using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WebApiFile.Attributes;
using WebApiFile.DB;
using WebApiFile.Enums;
using WebApiFile.Models;
using WebApiFile.Services.Email;
using File = WebApiFile.DB.Entities.File;

namespace WebApiFile.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class FileController : ControllerBase
    {

        private readonly ILogger<FileController> _logger;
        private readonly IEmailService _emailService;
        private readonly DataContext _dataContext;

        public FileController(
            ILogger<FileController> logger, 
            IEmailService emailService, 
            List<DeleteFileModel> codes,
            DataContext dataContext)
        {
            _logger = logger;
            _emailService = emailService;
            _dataContext = dataContext;
        }

        /// <summary>
        /// �������� ����� �� ������
        /// </summary>
        /// <remarks>
        /// �������� �����
        /// </remarks>
        /// <param name="file">���� ��� ��������</param>
        /// <response code="200">���� ������� ��������</response>
        /// <response code="400">���� �� ������</response>
        /// <response code="400">������ ���������</response>
        /// <response code="500">������ �������</response>
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [Authorize(Role.Developer, Role.Admin)]
        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file is null) return BadRequest();

            if (file.FileName.Length > 100)
                return BadRequest("������ ����� �� ����� ���� ������ 100 ��������");

            byte[] fileData = null;

            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
            {
                fileData = binaryReader.ReadBytes((int)file.Length);
            }

            var data = new File()
            {
                Name = file.FileName,
                Extension = file.FileName.Split('.').Last(),
                ContentType = file.ContentType,
                ContentDescription = file.ContentDisposition,
                Size = file.Length,
                Content = fileData,
            };

            await _dataContext.Files.AddAsync(data);
            await _dataContext.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Role.Developer, Role.Admin)]
        [HttpPost("UploadBase64")]
        public async Task<IActionResult> UploadBase64(string base64EncodedData, string fileName)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);

            var data = new File()
            {
                Content = base64EncodedBytes,
                Size = base64EncodedBytes.Length,
                Name = fileName,
                Extension = fileName.Split('.').Last(),
                ContentType = String.Empty,
                ContentDescription = String.Empty,
            };

            await _dataContext.Files.AddAsync(data);
            await _dataContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// ���������� �����
        /// </summary>
        /// <remarks>
        /// ���������� �����
        /// </remarks>
        /// <param name="id">ID ����� (Guid)</param>
        /// <response code="200">���� ������� ���������</response>
        /// <response code="400">���� �� ������</response>
        /// <response code="500">������ �������</response>
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [Authorize(Role.User, Role.Developer, Role.Editor, Role.Admin)]
        [HttpGet("Download")]
        public async Task<IActionResult> Download(Guid id)
        {
            var file = await _dataContext.Files.FindAsync(id);
            if (file is null) 
                return BadRequest();

            var cd = "attachment; filename=\"" + Uri.EscapeDataString(file.Name) + "\"";
            Response.Headers.Add("Content-Disposition", cd);
            return new FileContentResult(file.Content, "application/octet-stream");
        }

        /// <summary>
        /// ��������� ���������� �����
        /// </summary>
        /// <remarks>
        /// ��������� ���������� �����
        /// </remarks>
        /// <param name="id">ID ����� (Guid)</param>
        /// <response code="200">���������� ������� ���������</response>
        /// <response code="400">���� �� ������</response>
        /// <response code="500">������ �������</response>
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [Authorize(Role.User, Role.Developer, Role.Editor, Role.Admin)]
        [HttpGet("MetaData")]
        public async Task<IActionResult> GetMetaData(Guid id)
        {
            var file = await _dataContext.Files.FindAsync(id);
			if (file is null) return BadRequest("���� �� ������");

            return Ok(new FileMetaData()
            {
                ID = file.ID,
                Name = file.Name,
                ContentType = file.ContentType,
                ContentDescription = file.ContentDescription,
                Size = file.Size,
            });
        }

        /// <summary>
        /// ��������� �����
        /// </summary>
        /// <remarks>
        /// ��������� ����� �����
        /// </remarks>
        /// <param name="id">ID ����� (Guid)</param>
        /// <param name="newName">����� ��� �����</param>
        /// <response code="200">���� ������� �������</response>
        /// <response code="400">���� �� ������</response>
        /// <response code="400">������ ���������</response>
        /// <response code="500">������ �������</response>
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [Authorize(Role.Editor, Role.Admin)]
        [HttpPut("Change")]
        public async Task<IActionResult> Change(Guid id, string newName)
        {
            if (newName.Length > 100)
                return BadRequest("������ ����� �� ����� ���� ������ 100 ��������");

            var file = await _dataContext.Files.FindAsync(id);
            if (file is null) return BadRequest();

            string oldFileName = file.Name;
            file.Name = newName;

            var extension = newName.Split('.').LastOrDefault();

            if (extension != file.Extension)
                file.Name += "." + file.Extension;

            file.ContentDescription = file.ContentDescription.Replace(oldFileName, file.Name);

            await _dataContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("DeleteRequest")]
        public async Task<IActionResult> DeleteRequest(Guid id)
        {
            Random rnd = new Random();
            var newCode = rnd.Next(1000, 9999).ToString();
            await _dataContext.CodesForDelete.AddAsync(new DB.Entities.CodeForDelete()
            {
                FileId = id,
                Code = newCode,
                TimeTo = DateTime.UtcNow.AddMinutes(5)
            });

            await _dataContext.SaveChangesAsync();

            //await _emailService.SendEmailAsync(newCode);

            return Ok();
        }

        /// <summary>
        /// �������� �����
        /// </summary>
        /// <remarks>
        /// �������� �����
        /// </remarks>
        /// <param name="id">ID ����� (Guid)</param>
        /// <response code="200">���� ������� ������</response>
        /// <response code="400">���� �� ������</response>
        /// <response code="500">������ �������</response>
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [Authorize(Role.Editor, Role.Admin)]
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(Guid fileId, string code)
        {
            var filesToDelete = await _dataContext.CodesForDelete.Where(x => x.FileId == fileId).ToListAsync();
            if(filesToDelete != null && filesToDelete.Any(x => x.Code == code && x.TimeTo > DateTime.UtcNow))
            {
                var filesId = filesToDelete.ConvertAll(x => x.ID);
                await _dataContext.CodesForDelete.Where(x => filesId.Contains(x.ID)).ExecuteDeleteAsync();
                await _dataContext.Files.Where(x => x.ID == fileId).ExecuteDeleteAsync();

                return Ok($"���� � ��������� ID ������ {DateTime.Now.ToLocalTime()}");
            }

            return BadRequest();
        }
    }
}