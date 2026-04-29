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

            int diagnosisNormalCount = 0;
            int classificationNormalCount = 0;
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
                    diagnosisNormalCount++;
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
                    classificationNormalCount++;
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


            if (diagnosisNormalCount >= 10)
            {
                if (classificationNormalCount < 2)
                    result.Diagnosis = "Normal (retest recommended)";
                else
                    result.Diagnosis = "Normal";
            }
            else if (diagnosisNormalCount < 8)
            {
                if (protanCount > deutanCount)
                    result.Diagnosis = "protan";
                else if (deutanCount > protanCount)
                    result.Diagnosis = "deutan";
                else
                    result.Diagnosis = "uncertainty";
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
                CorrectAnswerCount = diagnosisNormalCount + classificationNormalCount,
                DeutanAnswerCount = deutanCount,
                ProtanAnswerCount = protanCount
            };

            return response;
        }
    }
}