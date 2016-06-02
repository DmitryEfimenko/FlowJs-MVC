using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using FlowJs;
using FlowJs.Interface;
using Microsoft.AspNet.Identity;

namespace MyProject.Controllers
{
    [Authorize]
    [RoutePrefix("api/File")]
    public class FileController : ApiController
    {
        private readonly IFileManagerService _fileManager;
        private readonly IFlowJsRepo _flowJs;

        public PictureController()
        {
            _fileManager = new FileManagerService();
            _flowJs = new FlowJsRepo();
        }

        const string Folder = @"C:\Temp\PicUpload";

        [HttpGet]
        [Route("Upload")]
        public async Task<IHttpActionResult> PictureUploadGet()
        {
            var request = HttpContext.Current.Request;

            var chunkExists = _flowJs.ChunkExists(Folder, request);
            if (chunkExists) return Ok();
            return ResponseMessage(new HttpResponseMessage(HttpStatusCode.NoContent));
        }

        [HttpPost]
        [Route("Upload")]
        public async Task<IHttpActionResult> PictureUploadPost()
        {
            var request = HttpContext.Current.Request;
            
            var validationRules = new FlowValidationRules();
            validationRules.AcceptedExtensions.AddRange(new List<string> { "jpeg", "jpg", "png", "bmp" });
            validationRules.MaxFileSize = 5000000;

            try
            {
                var status = _flowJs.PostChunk(request, Folder, validationRules);
    
                if (status.Status == PostChunkStatus.Done)
                {
                    // file uploade is complete. Below is an example of further file handling
                    var filePath = Path.Combine(Folder, status.FileName);
                    var file = File.ReadAllBytes(filePath);
                    var picture = await _fileManager.UploadPictureToS3(User.Identity.GetUserId(), file, status.FileName);
                    File.Delete(filePath);
                    return Ok(picture);
                }
    
                if (status.Status == PostChunkStatus.PartlyDone)
                {
                    return Ok();
                }
    
                status.ErrorMessages.ForEach(x => ModelState.AddModelError("file", x));
                return BadRequest(ModelState);
            }
            catch (Exception)
            {
                ModelState.AddModelError("file", "exception");
                return BadRequest(ModelState);
            }
        }
    }
}
