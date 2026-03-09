using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UniversityClubSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Sadece giriş yapanlar dosya yükleyebilir
    public class MediaController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public MediaController(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Belirtilen klasöre (clubs, events, universities) resim yüklemesi yapar.
        /// </summary>
        /// <param name="folder">Hedef klasör adı</param>
        /// <param name="file">Yüklenecek dosya</param>
        /// <returns>Dosyanın erişim URL'si</returns>
        [HttpPost("upload/{folder}")]
        public async Task<IActionResult> Upload(string folder, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // İzin verilen klasörler
            var allowedFolders = new[] { "clubs", "events", "universities" };
            if (!allowedFolders.Contains(folder.ToLower()))
                return BadRequest("Invalid folder name.");

            // Dosya uzantısı kontrolü
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Invalid file type. Only images are allowed.");

            // Benzersiz dosya adı oluştur
            var fileName = $"{Guid.NewGuid()}{extension}";
            var path = Path.Combine(_env.WebRootPath, "uploads", folder, fileName);

            // Dosyayı kaydet
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Erişim URL'sini döndür
            var url = $"/uploads/{folder}/{fileName}";
            return Ok(new { url });
        }
    }
}
