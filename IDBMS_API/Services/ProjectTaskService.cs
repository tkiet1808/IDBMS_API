﻿using Azure.Core;
using IDBMS_API.DTOs.Request;
using BusinessObject.Enums;
using BusinessObject.Models;
using Repository.Interfaces;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Office2016.Excel;
using System.Text;
using System.Globalization;
using UnidecodeSharpFork;

namespace IDBMS_API.Services
{
    public class ProjectTaskService
    {
        private readonly IProjectTaskRepository _taskRepo;
        private readonly IProjectRepository _projectRepo;
        private readonly IPaymentStageRepository _stageRepo;
        private readonly IProjectDesignRepository _projectDesignRepo;
        private readonly IPaymentStageDesignRepository _stageDesignRepo;

        public ProjectTaskService(
            IProjectTaskRepository taskRepo,
            IProjectRepository projectRepo,
            IPaymentStageRepository stageRepo,
            IProjectDesignRepository projectDesignRepo,
            IPaymentStageDesignRepository stageDesignRepo)
        {
            _taskRepo = taskRepo;
            _projectRepo = projectRepo;
            _stageRepo = stageRepo; 
            _projectDesignRepo = projectDesignRepo;
            _stageDesignRepo = stageDesignRepo;
        }

/*        private string NormalizeString(string input)
        {
            if (input == null)
            {
                return string.Empty;
            }

            input = input.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in input)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(char.ToLower(c));
                }
            }

            return sb.ToString();
        }*/

        public IEnumerable<ProjectTask> Filter(IEnumerable<ProjectTask> list, 
            string? codeOrName, Guid? stageId, ProjectTaskStatus? taskStatus, int? taskCategoryId, Guid? roomId)
        {
            IEnumerable<ProjectTask> filteredList = list;

            if (codeOrName != null)
            {
                filteredList = filteredList.Where(item =>
                            (item.Code != null && item.Code.Unidecode().IndexOf(codeOrName.Unidecode(), StringComparison.OrdinalIgnoreCase) >= 0) ||
                            (item.Name != null && item.Name.Unidecode().IndexOf(codeOrName.Unidecode(), StringComparison.OrdinalIgnoreCase) >= 0));
            }

            if (stageId != null)
            {
                filteredList = filteredList.Where(item => item.PaymentStageId == stageId);
            }

            if (taskStatus != null)
            {
                filteredList = filteredList.Where(item => item.Status == taskStatus);
            }

            if (taskCategoryId != null)
            {
                filteredList = filteredList.Where(item => item.TaskCategoryId == taskCategoryId);
            }

            if (roomId != null)
            {
                filteredList = filteredList.Where(item => item.RoomId == roomId);
            }

            return filteredList;
        }
        
        public IEnumerable<ProjectTask> GetAll()
        {
            return _taskRepo.GetAll();
        }

        public ProjectTask? GetById(Guid id)
        {
            return _taskRepo.GetById(id) ?? throw new Exception("This object is not existed!");
        }

        public IEnumerable<ProjectTask?> GetByProjectId(Guid id, 
            string? codeOrName, Guid? stageId, ProjectTaskStatus? taskStatus, int? taskCategoryId, Guid? roomId)
        {
            var list = _taskRepo.GetByProjectId(id);
            
            return Filter(list, codeOrName, stageId, taskStatus, taskCategoryId, roomId);
        }

        public IEnumerable<ProjectTask?> GetByRoomId(Guid id)
        {
            return _taskRepo.GetByRoomId(id);
        }

        public IEnumerable<ProjectTask?> GetByPaymentStageId(Guid id)
        {
            return _taskRepo.GetByPaymentStageId(id);
        }

        public void UpdateProjectData(Guid projectId)
        {
            var tasksInProject = _taskRepo.GetByProjectId(projectId);

            decimal estimatePrice = 0;
            decimal finalPrice = 0;
            int estimateBusinessDay = 0;

            if (tasksInProject != null && tasksInProject.Any())
            {
                estimatePrice = tasksInProject.Sum(task =>
                {
                    if (task != null && task.Status != ProjectTaskStatus.Cancelled && task.IsIncurred != true)
                    {
                        decimal pricePerUnit = task.PricePerUnit;
                        double unitInContract = task.UnitInContract;
                        return pricePerUnit * (decimal)unitInContract;
                    }
                    return 0; 
                });

                finalPrice = tasksInProject.Sum(task =>
                {
                    if (task != null && task.Status != ProjectTaskStatus.Cancelled)
                    {
                        decimal pricePerUnit = task.PricePerUnit;
                        double unitUsed = (task.UnitUsed > task.UnitInContract ? task.UnitUsed : task.UnitInContract);
                        return pricePerUnit * (decimal)unitUsed;
                    }
                    return 0;
                });

                estimateBusinessDay = tasksInProject.Sum(task =>
                {
                    if (task != null && task.Status != ProjectTaskStatus.Cancelled)
                    {
                        return task.EstimateBusinessDay;
                    }
                    return 0;
                });
            }

            ProjectService projectService = new(_projectRepo);
            projectService.UpdateProjectDataByTask(projectId, estimatePrice, finalPrice, estimateBusinessDay);

        }

        public void UpdatePaymentStageData(Guid projectId)
        {
            PaymentStageService stageService = new(_stageRepo, _projectRepo, _projectDesignRepo, _stageDesignRepo);
            var stagesByProjectId = stageService.GetByProjectId(projectId);

            foreach (var stage in stagesByProjectId)
            {
                var tasksInStage = _taskRepo.GetByPaymentStageId(projectId);
                int estimateBusinessDay = 0;

                if (tasksInStage != null && tasksInStage.Any())
                {
                    estimateBusinessDay = tasksInStage.Sum(task =>
                    {
                        if (task != null && task.Status != ProjectTaskStatus.Cancelled)
                        {
                            return task.EstimateBusinessDay;
                        }
                        return 0;
                    });
                }

                stageService.UpdateStagesDataByTask(stage.Id, estimateBusinessDay);
            }
        }

        public void UpdateTaskProgress(Guid taskId, double unitUsed)
        {
            var ct = _taskRepo.GetById(taskId) ?? throw new Exception("This object is not existed!");

            ct.UnitUsed = unitUsed;
            ct.Percentage = (int)((unitUsed / ct.UnitInContract) * 100);

            _taskRepo.Update(ct);
        }

        public ProjectTask? CreateProjectTask(ProjectTaskRequest request)
        {
            var ct = new ProjectTask
            {
                Id = Guid.NewGuid(),
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                CalculationUnit = request.CalculationUnit,
                PricePerUnit = request.PricePerUnit,
                UnitInContract = request.UnitInContract,
                UnitUsed = 0,
                IsIncurred = request.IsIncurred,
                StartedDate = request.StartedDate,
                EndDate = request.EndDate,
                CreatedDate = DateTime.Now,
                ProjectId = request.ProjectId,
                PaymentStageId = request.PaymentStageId,
                RoomId = request.RoomId,
                Status = request.Status,
                EstimateBusinessDay = request.EstimateBusinessDay,
            };
            var ctCreated = _taskRepo.Save(ct);

            UpdateProjectData(request.ProjectId);
            UpdatePaymentStageData(request.ProjectId);

            return ctCreated;
        }

        public void AssignTasksToStage(Guid paymentStageId, List<Guid> listTaskId, Guid projectId)
        {
                foreach (var taskId in listTaskId)
                {
                    var task = _taskRepo.GetById(taskId) ?? throw new Exception("This object is not existed!");

                    task.PaymentStageId = paymentStageId;

                    _taskRepo.Update(task);
                }
            UpdatePaymentStageData(projectId);
        }

        public void StartTasksOfStage(Guid paymentStageId, Guid projectId)
        {
            var listTask = _taskRepo.GetByPaymentStageId(paymentStageId);
            if (listTask.Any())
            {
                foreach (var task in listTask)
                {
                    if (task!= null && task.PaymentStageId == paymentStageId && task.Status == ProjectTaskStatus.Confirmed)
                    {
                        task.StartedDate = DateTime.Now;
                        _taskRepo.Update(task);
                    }
                }
            }

            UpdatePaymentStageData(projectId);
        }

        public void UpdateProjectTask(Guid id, ProjectTaskRequest request)
        {
            var ct = _taskRepo.GetById(id) ?? throw new Exception("This object is not existed!");

            ct.Code = request.Code;
            ct.Name = request.Name;
            ct.Description = request.Description;
            ct.CalculationUnit = request.CalculationUnit;
            ct.PricePerUnit = request.PricePerUnit;
            ct.UnitInContract = request.UnitInContract;
            ct.IsIncurred = request.IsIncurred;
            ct.UpdatedDate= DateTime.Now;
            ct.EndDate = request.EndDate;
            ct.ProjectId = request.ProjectId;
            ct.PaymentStageId = request.PaymentStageId;
            ct.RoomId = request.RoomId;
            ct.Status = request.Status;
            ct.EstimateBusinessDay= request.EstimateBusinessDay;

            _taskRepo.Update(ct);

            UpdateProjectData(request.ProjectId);
            UpdatePaymentStageData(request.ProjectId);
        }
        public void UpdateProjectTaskStatus(Guid id, ProjectTaskStatus status)
        {
            var ct = _taskRepo.GetById(id) ?? throw new Exception("This object is not existed!");

            ct.Status = status;

            _taskRepo.Update(ct);

            UpdateProjectData(ct.ProjectId);
            UpdatePaymentStageData(ct.ProjectId);
        }

    }
}
