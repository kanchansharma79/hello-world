using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogBug.Library;
using Microsoft.TeamFoundation.Client;

namespace LogBug
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionUrl = null, token = null, project = null;
            //args = new string[] { "/url:http://cmp02-app05:8080/tfs/DefaultCollection/", "/project:WebSphere" };
            args = new string[] { "/url:https://kanchansharmaqa.visualstudio.com/DefaultCollection/", "/project:Training" };

            try
            {
                CheckArguments(args, out connectionUrl, out project);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Executing quick start sample to create a Bug...");
            Console.WriteLine("");

            string BugTitle = "Demo bug title TFS API"; string Description = "This is a demo 1 bug description"; int Priority = 3;
            string Area = @"Training"; string Iteration = @"Training\Release 1\Sprint 1"; string ReproSteps = "Click the screen";
            string Assignee = "Kanchan Sharma";

            //todo: Create a new bug using TFS API.
            TfsUtility tfsUtility = new TfsUtility();
            
            //Project tfsProject = tfsUtility.GetTeamProject(connectionUrl, project);
            var result = tfsUtility.CreateBug(connectionUrl, project, BugTitle, Description, Area, Iteration, Assignee, ReproSteps);
        }

        private static void CheckArguments(string[] args, out string connectionUrl, out string project)
        {
            connectionUrl = null;
            project = null;

            Dictionary<string, string> argsMap = new Dictionary<string, string>();
            foreach (var arg in args)
            {
                if (arg[0] == '/' && arg.IndexOf(':') > 1)
                {
                    string key = arg.Substring(1, arg.IndexOf(':') - 1);
                    string value = arg.Substring(arg.IndexOf(':') + 1);

                    switch (key)
                    {
                        case "url":
                            connectionUrl = value;
                            break;

                        case "project":
                            project = value;
                            break;
                        default:
                            throw new ArgumentException("Unknown argument", key);
                    }
                }
            }

            if (connectionUrl == null)
                throw new ArgumentException("Missing required arguments");

        }
    }
}
