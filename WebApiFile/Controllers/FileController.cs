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
        /// Загрузка файла на сервер
        /// </summary>
        /// <remarks>
        /// Загрузка файла
        /// </remarks>
        /// <param name="file">Файл для загрузки</param>
        /// <response code="200">Файл успешно загружен</response>
        /// <response code="400">Файл не найден</response>
        /// <response code="400">Ошибка валидации</response>
        /// <response code="500">Ошибка сервера</response>
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [Authorize(Role.Developer, Role.Admin)]
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file is null) return BadRequest();

            if (file.FileName.Length > 100)
                return BadRequest("Длинна файла не может быть больше 100 символов");

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
        /// Скачивание файла
        /// </summary>
        /// <remarks>
        /// Скачивание файла
        /// </remarks>
        /// <param name="id">ID файла (Guid)</param>
        /// <response code="200">Файл успешно отправлен</response>
        /// <response code="400">Файл не найден</response>
        /// <response code="500">Ошибка сервера</response>
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
        /// Получение метаданных файла
        /// </summary>
        /// <remarks>
        /// Получение метаданных файла
        /// </remarks>
        /// <param name="id">ID файла (Guid)</param>
        /// <response code="200">Метаданные успешно отправлен</response>
        /// <response code="400">Файл не найден</response>
        /// <response code="500">Ошибка сервера</response>
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [Authorize(Role.User, Role.Developer, Role.Editor, Role.Admin)]
        [HttpGet("MetaData")]
        public async Task<IActionResult> GetMetaData(Guid id)
        {
            var file = await _dataContext.Files.FindAsync(id);
			if (file is null) return BadRequest("Файл не найден");

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
        /// Изменение файла
        /// </summary>
        /// <remarks>
        /// Изменение имени файла
        /// </remarks>
        /// <param name="id">ID файла (Guid)</param>
        /// <param name="newName">Новое имя файла</param>
        /// <response code="200">Файл успешно изменен</response>
        /// <response code="400">Файл не найден</response>
        /// <response code="400">Ошибка валидации</response>
        /// <response code="500">Ошибка сервера</response>
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [Authorize(Role.Editor, Role.Admin)]
        [HttpPut("Change")]
        public async Task<IActionResult> Change(Guid id, string newName)
        {
            if (newName.Length > 100)
                return BadRequest("Длинна файла не может быть больше 100 символов");

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

        [HttpPost("SendCode")]
        public async Task<IActionResult> SendCode(Guid id)
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
        /// Удаление файла
        /// </summary>
        /// <remarks>
        /// Удаление файла
        /// </remarks>
        /// <param name="id">ID файла (Guid)</param>
        /// <response code="200">Файл успешно удален</response>
        /// <response code="400">Файл не найден</response>
        /// <response code="500">Ошибка сервера</response>
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [Authorize(Role.Editor, Role.Admin)]
        [HttpDelete("{id}&{code}")]
        public async Task<IActionResult> Delete(Guid id, string code)
        {
            var filesToDelete = await _dataContext.CodesForDelete.Where(x => x.FileId == id).ToListAsync();
            if(filesToDelete != null && filesToDelete.Any(x => x.Code == code && x.TimeTo > DateTime.UtcNow))
            {
                var filesId = filesToDelete.ConvertAll(x => x.ID);
                await _dataContext.CodesForDelete.Where(x => filesId.Contains(x.ID)).ExecuteDeleteAsync();
                await _dataContext.Files.Where(x => x.ID == id).ExecuteDeleteAsync();

                return Ok($"Файл с указанным ID удален {DateTime.Now.ToLocalTime()}");
            }

            return BadRequest();
        }
    }
}