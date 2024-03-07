"use stritct"
var openMeetingNotesPopup = function (id) {
    $('#meetingNoteModal').data("MeetingId", id);
    $('#meetingNoteModal').modal('show')
}
var createNewMeeting = function () {
    showLoader();
    const meetingRequest = {
        topic: "Interview",
        agenda: "",
        type: 2,
        start_time: new Date(), // assuming startTime is current date and time in ISO format
        duration: 40,
        settings: {
            host_video: false,
            participant_video: false,
            join_before_host: false,
            mute_upon_entry: false,
            watermark: false,
            audio: "voip"
        }
    };

    $.ajax({
        url: "/Home/CreateMeeting",
        type: "POST",
        data: meetingRequest
        ,
        success: function (resp) {
            if (resp.status == true) {
                $('#zoom').empty();

                let html = ` <div class="comment" style="border-left: 5px solid green !important;">
        <div class="user-info"><a target="_blank" href="${resp.joinUrl}">Join Zoom Meeting</a></div>
    </div>`
                $('#zoom').append(html)
            }
        },
        error: function (error) {
            // Handle error

            console.log(error);
        }, complete: function () {
            hideLoader();
        }
    });
}
function addMeetingNotes() {
    showLoader();
    let meetingId = $('#meetingNoteModal').data("MeetingId");
    $.ajax({
        url: "/Home/AddMeetingNote",
        type: "POST",
        data: { id: meetingId, note: $('#txtmeetingNotes').val() }
        ,
        success: function (data) {
            window.location.reload();
        },
        error: function (error) {
            // Handle error

            console.log(error);
        }, complete: function () {
            hideLoader();
        }
    });
}
function copyMeetingLink(joinUrl) {
    var tempInput = document.createElement("input");

    tempInput.setAttribute("value", joinUrl);

    document.body.appendChild(tempInput);

    tempInput.select();

    document.execCommand("copy");

    document.body.removeChild(tempInput);

    alert("Meeting link copied to clipboard");
}
function confirmDelete(id) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            showLoader();
            $.ajax({
                url: "/Home/DeleteMeeting",
                type: "POST",
                data: { id: id }
                ,
                success: function (data) {
                    debugger
                    $('#row_' + id).remove();
                    Swal.fire({
                        title: "Deleted!",
                        text: "Your meeting has been deleted.",
                        icon: "success"
                    });
                },
                error: function (error) {
                    // Handle error

                    console.log(error);
                }, complete: function () {
                    hideLoader();
                }
            });
        }
    });
}