using ColorMate.Core.DTOs;
using ColorMate.Core.Models;
using ColorMate.EF.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace ColorMate.BL.TestService
{
    public class TestService : ITestService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public TestResultDto CalculateTestResult(string userId, List<TestAnswersDto> testAnswersDto)
        {
            var result = new TestResult
            {
                ApplicationUserId = userId,
                TestTime = DateTime.Now,
                Answers = new List<UserAnswer>()
            };

            var _plates = _unitOfWork.TestQuestions.GetAll().ToDictionary(q => q.ImageId);

            int normalCount = 0;
            int protanCount = 0;
            int deutanCount = 0;

            foreach (var answer in testAnswersDto.Where(t => t.UsedForDiagnosis))
            {
                if (!_plates.ContainsKey(answer.ImageId)) continue;

                var questionDB = _plates[answer.ImageId];

                var userAnswer = new UserAnswer
                {
                    TestQuestionId = questionDB.Id,
                    Answer = answer.Value,
                    TestResultId = result.Id
                };

                if (answer.Value == questionDB.NormalAnswer)
                {
                    userAnswer.IsCorrect = true;
                    normalCount++;
                }
                else
                {
                    userAnswer.IsCorrect = false;
                }

                result.Answers.Add(userAnswer);

            }



            foreach (var answer in testAnswersDto.Where(t => t.UsedForDiagnosis == false))
            {
                if (!_plates.ContainsKey(answer.ImageId)) continue;

                var questionDB = _plates[answer.ImageId];

                var userAnswer = new UserAnswer
                {
                    TestQuestionId = questionDB.Id,
                    Answer = answer.Value,
                    TestResultId = result.Id
                };

                if (answer.Value == questionDB.NormalAnswer)
                {
                    userAnswer.IsCorrect = true;
                    normalCount++;
                }
                else
                {
                    userAnswer.IsCorrect = false;

                    if (answer.Value == questionDB.ProtanAnswer)
                    {
                        protanCount++;
                    }
                    else if (answer.Value == questionDB.DeutanAnswer)
                    {
                        deutanCount++;
                    }
                }

                result.Answers.Add(userAnswer);
            }


            if (normalCount >= 10)
            {
                result.Diagnosis = "Normal";
            }
            else if (normalCount <= 7)
            {
                if (protanCount > deutanCount)
                {
                    result.Diagnosis = "protan";
                }
                else
                {
                    result.Diagnosis = "deutan";
                }
            }
            else
            {
                result.Diagnosis = "uncertainty";
            }

            _unitOfWork.TestResults.Add(result);
            _unitOfWork.Complete();

            var response = new TestResultDto
            {
                TestTime = DateTime.Now,
                Diagnosis = result.Diagnosis,
                CorrectAnswerCount = normalCount,
                DeutanAnswerCount = deutanCount,
                ProtanAnswerCount = protanCount
            };

            return response;
        }
    }
}
