﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Webdictaat.Api.Services;
using Webdictaat.Api.ViewModels.Assignments;
using Webdictaat.Data;
using Webdictaat.Domain.Assignments;

namespace Webdictaat.Api.Models
{
    public interface IAssignmentRepository
    {
        AssignmentVM GetAssignment(int assignmentId, string userId = null);

        AssignmentVM CreateAssignment(string dictaatName, AssignmentFormVM form);

        /// <summary>
        /// Complete a assignment. 
        /// Admins can complete an assignment, or users that know the assignment secret
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="assignmentId"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        AssignmentVM CompleteAssignment(int assignmentId, string userId);
        AssignmentVM CompleteAssignment(int assignmentId, string userId, string token);
    }

    public class AssignmentRepository : IAssignmentRepository
    {
        private WebdictaatContext _context;
        private ISecretService _secretService;

        public AssignmentRepository(WebdictaatContext context, ISecretService secretService)
        {
            _context = context;
            _secretService = secretService;
        }

        public AssignmentVM CompleteAssignment(int assignmentId, string userId)
        {
            var assignment = _context.Assignments.FirstOrDefault(a => a.Id == assignmentId);

            if(assignment != null)
            {
                completeAssignment(assignment, userId);
            }

            return GetAssignment(assignmentId, userId);
        }

        public AssignmentVM CompleteAssignment(int assignmentId, string userId, string token)
        {
            Assignment assignment = _context.Assignments.FirstOrDefault(a => a.Id == assignmentId);

            if (assignment != null)
            {
                var assignmentToken = _secretService.GetAssignmentToken(userId, assignmentId, assignment.AssignmentSecret);
                if(token == assignmentToken)
                {
                    completeAssignment(assignment, userId);
                }
          
                return GetAssignment(assignmentId, userId);
            }

            return null;
        }

        private AssignmentSubmission completeAssignment(Assignment assignment, string userId) {
            var mySubmission = _context.AssignmentSubmissions.FirstOrDefault(a => a.UserId == userId && a.AssignmentId == assignment.Id);

            if (mySubmission != null)
            {
                return mySubmission;
            }

            var submission = new AssignmentSubmission()
            {
                AssignmentId = assignment.Id,
                UserId = userId,
                Timestamp = DateTime.Now,
                PointsRecieved = assignment.Points,
            };

            _context.AssignmentSubmissions.Add(submission);
            _context.SaveChanges();
            return submission;

        }

      

        public AssignmentVM CreateAssignment(string dictaatName, AssignmentFormVM form)
        {

            throw new NotImplementedException();
        }

        public AssignmentVM GetAssignment(int assignmentId, string userId = null)
        {
            var assignment = _context.Assignments.FirstOrDefault(a => a.Id == assignmentId);
            var response = new AssignmentVM(assignment);

            if(userId != null)
            {
                response.MySubmission = _context.AssignmentSubmissions.FirstOrDefault(a => a.UserId == userId && a.AssignmentId == assignmentId);
            }

            return response;

        }
    }
}