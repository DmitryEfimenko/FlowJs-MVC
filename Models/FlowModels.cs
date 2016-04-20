using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;

namespace FlowJs
{
    public enum PostChunkStatus
    {
        Error = 0,
        Done = 1,
        PartlyDone = 2
    }
    public class FlowJsPostChunkResponse
    {
        public FlowJsPostChunkResponse()
        {
            ErrorMessages = new List<string>();
        }

        public string FileName { get; set; }
        public long Size { get; set; }
        public PostChunkStatus Status { get; set; }
        public List<string> ErrorMessages { get; set; }
    }

    public class FlowChunk
    {
        public int Number { get; set; }
        public long Size { get; set; }
        public long TotalSize { get; set; }
        public string Identifier { get; set; }
        public string FileName { get; set; }
        public int TotalChunks { get; set; }

        internal bool ParseForm(NameValueCollection form)
        {
            try
            {
                if (string.IsNullOrEmpty(form["flowIdentifier"]) || string.IsNullOrEmpty(form["flowFilename"]))
                    return false;

                Number = int.Parse(form["flowChunkNumber"]);
                Size = long.Parse(form["flowChunkSize"]);
                TotalSize = long.Parse(form["flowTotalSize"]);
                Identifier = CleanIdentifier(form["flowIdentifier"]);
                FileName = form["flowFilename"];
                TotalChunks = int.Parse(form["flowTotalChunks"]);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        internal bool ValidateBusinessRules(FlowValidationRules rules, out List<string> errorMessages)
        {
            errorMessages = new List<string>();
            if(rules.MaxFileSize.HasValue && TotalSize > rules.MaxFileSize.Value)
                errorMessages.Add(rules.MaxFileSizeMessage ?? "size");

            if (rules.AcceptedExtensions.Count > 0 && rules.AcceptedExtensions.SingleOrDefault(x => x.ToLowerCase() == FileName.Split('.').Last().ToLowerCase()) == null)
                errorMessages.Add(rules.AcceptedExtensionsMessage ?? "type");

            return errorMessages.Count == 0;
        }

        private string CleanIdentifier(string identifier)
        {
            identifier = Regex.Replace(identifier, "/[^0-9A-Za-z_-]/g", "");
            return identifier;
        }


        
    }

    public class FlowValidationRules
    {
        public FlowValidationRules()
        {
            AcceptedExtensions = new List<string>();
        }
        
        public long? MaxFileSize { get; set; }
        public string MaxFileSizeMessage { get; set; }

        public List<string> AcceptedExtensions { get; set; }
        public string AcceptedExtensionsMessage { get; set; }
    }
}
