using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NuGet.Configuration;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using ZoomAPI.Data;
using ZoomAPI.Interfaces;
using ZoomAPI.Models;

namespace ZoomAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Zoom _zoomConfig;
        private readonly IZoom _zoomRepository;
        public HomeController(ILogger<HomeController> logger, IOptions<Zoom> zoomConfig, IZoom zoom)
        {
            _logger = logger;
            _zoomConfig = zoomConfig.Value;
            _zoomRepository = zoom;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ConnectZoom()
        {
            var url = $"https://zoom.us/oauth/authorize?response_type=code&client_id={_zoomConfig.app_id}&redirect_uri={_zoomConfig.redirect_uri}";
            return Redirect(url);
        }
        [HttpPost]
        public async Task<IActionResult> DisconnectZoom()
        {
            await _zoomRepository.DisconnectZoom();
            return View("Index");
        }
        public async Task<IActionResult> Authenticate(string code)
        {
            await _zoomRepository.Authenticate(code);
            return View("Index");
        }
        [ActionName("Meetings")]
        public async Task<IActionResult> GetMeetings()
        {
           var meetings= await _zoomRepository.GetMeetings();
            return View("GetMeetings", meetings);
        }
        public  IActionResult CreateMeeting()
        {         
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateMeeting(MeetingRequest meetingRequest)
        {
            if (ModelState.IsValid)
            {
                var result = await _zoomRepository.CreateMeeting(meetingRequest);
                if (result.status)
                {
                    return Json(new {status=true,joinUrl=result.joinUrl});
                }
                else
                {
                    return Json(new { status = false, error = result.error });
                }
            }
            return Json(new { status = false, error = "Invalid Request" });
        }
        
        [HttpPost]
        public async Task<IActionResult> AddMeetingNote(string id,string note)
        {
             var meetings = await _zoomRepository.UpdateMeeting(id, note);
                if (meetings)
                {
                    return Redirect("/Home/Meetings");
                }
            
            return View();

        }
        [HttpPost]
        public async Task<IActionResult> DeleteMeeting(string id)
        {
            var status = await _zoomRepository.DeleteMeeting(id);
            if (status)
            {
                return Json(true);
            }

            return Json(false);

        }

    }
}
