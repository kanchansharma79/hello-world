using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.TeamFoundation.Client;

namespace LogBug.Library
{
    class TfsUtility
    {
        public TfsUtility()
        {
        }

        #region TFS API
        public ActionResult CreateBug(string uri, string teamProjectName, string title, string description,
            string area, string iteration, string assignee, string reproductionSteps)
        {

            #region Temp
            // Connect to the server and the store, and get the WorkItemType object
            // for Bug from the team project where the user story will be created. 
            Uri collectionUri = new Uri(uri);//"http://server:port/vdir/DefaultCollection");
            TfsTeamProjectCollection tpc = new TfsTeamProjectCollection(collectionUri);
            WorkItemStore workItemStore = tpc.GetService<WorkItemStore>();
            Project teamProject = workItemStore.Projects[teamProjectName];
            WorkItemType workItemType = teamProject.WorkItemTypes["Bug"];

            // Create the work item. 
            WorkItem newBug = new WorkItem(workItemType);
            {
                // The title is generally the only required field that doesn’t have a default value. 
                // You must set it, or you can’t save the work item. If you’re working with another
                // type of work item, there may be other fields that you’ll have to set.
                // Create the work item.             
                newBug.Title = title;
                newBug.Description = description;
                newBug.AreaPath = area;
                newBug.IterationPath = iteration;
                newBug.Fields["Priority"].Value = 2;
                newBug.Fields["Severity"].Value = "4 - Low";
                newBug.Fields["Assigned To"].Value = assignee;
                newBug.Fields["Repro Steps"].Value = reproductionSteps;
                newBug.Attachments.Add(new Attachment(@"D:\Usha.txt"));
            };
            //// Save the new user story. 
            //newBug.Save();
            #endregion
            
            var validationResult = newBug.Validate();

            if (validationResult.Count == 0)
            {
                // Save the new work item.
                newBug.Save();
                
                return new ActionResult()
                {
                    Success = true,
                    newWorkItem = newBug
                };
            }
            else
            {
                // Establish why it can't be saved
                var result = new ActionResult()
                {
                    Success = false,
                    ErrorCodes = new List<string>()
                };

                foreach (var res in validationResult)
                {
                    Microsoft.TeamFoundation.WorkItemTracking.Client.Field field = res as Microsoft.TeamFoundation.WorkItemTracking.Client.Field;
                    if (field == null)
                    {
                        result.ErrorCodes.Add(res.ToString());
                    }
                    else
                    {
                        result.ErrorCodes.Add("Error with: {field.Name}");
                    }
                }
                return result;
            }
        }

        public Project GetTeamProject(string uri, string projectName)
        {
            TfsTeamProjectCollection tfs;
            tfs = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(uri)); // https://mytfs.visualstudio.com/DefaultCollection
            tfs.Authenticate();

            var workItemStore = new WorkItemStore(tfs);

            var project = (from Project pr in workItemStore.Projects
                           where pr.Name == projectName
                           select pr).FirstOrDefault();
            if (project == null)
                throw new Exception("Unable to find {name} in {uri}");

            return project;
        }
        #endregion
    }
    public class ActionResult
    {
        public bool Success { get; set; }
        public WorkItem newWorkItem { get; set; }
        public List<string> ErrorCodes { get; set; }
    }
}
