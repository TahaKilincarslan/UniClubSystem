using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityClubSystem.DTOs;
using UniversityClubSystem.Models;
using UniversityClubSystem.Services;

namespace UniversityClubSystem.Controllers
{
    [ApiController]
    [Authorize(Roles = $"{nameof(UserRole.ClubManager)},{nameof(UserRole.SystemAdmin)}")]
    public class AdminEventsController : ControllerBase
    {
        private readonly EventRequestService _eventRequestService;

        public AdminEventsController(EventRequestService eventRequestService)
        {
            _eventRequestService = eventRequestService;
        }

        /// <summary>
        /// Bir etkinliğe gelen tüm katılma isteklerini listeler. (Yalnızca Admin)
        /// GET /api/admin/events/{eventId}/requests
        /// </summary>
        [HttpGet("api/admin/events/{eventId:int}/requests")]
        public async Task<IActionResult> GetRequestsForEvent(int eventId)
        {
            var requests = await _eventRequestService.GetRequestsByEventIdAsync(eventId);
            return Ok(requests);
        }

        /// <summary>
        /// Bir katılma isteğini onaylar veya reddeder. (Yalnızca Admin)
        /// PUT /api/admin/event-requests/{requestId}/status
        /// </summary>
        [HttpPut("api/admin/event-requests/{requestId:int}/status")]
        public async Task<IActionResult> UpdateRequestStatus(int requestId, [FromBody] UpdateEventRequestStatusDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Status))
                return BadRequest(new { message = "Statü boş olamaz. Geçerli değerler: Pending, Approved, Rejected." });

            var (success, message) = await _eventRequestService.UpdateRequestStatusAsync(requestId, dto.Status);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }
    }
}
