using ZoomAPI.Models;

namespace ZoomAPI.Interfaces
{
    public interface IZoom
    {
        Task<bool> IsConnected();
        Task<bool> DisconnectZoom();
        Task<bool> Authenticate(string code);
        Task<Root> GetMeetings();
        Task<(bool status, string error, string joinUrl)> CreateMeeting(MeetingRequest meetingRequest);
        Task<bool> UpdateMeeting(string meetingId, string note);
        Task<bool> DeleteMeeting(string meetingId);
        Task<string> RefreshTokenAsync(string refreshToken);
        Task<bool> SaveRefreshToken(string refreshToken);
        Task<string> GetRefreshToken();
    }
}
