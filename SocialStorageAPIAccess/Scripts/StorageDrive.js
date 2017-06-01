function OnFileServiceTypeSelect(sender) {
    window.location.href = "/StorageDrive/Index?type=" + sender.value;
}
function GetList(folderName, folderId, isRefreshCurrent) {
    $("#hdnFolderName").val(folderName);
    var dataToSend = { folderId: folderId, type: $("#hdnCurrentSelectedType").val() };
    $("#hdnCurrentSelectedPath").val(folderId);
    $("#divGetFolderList").html("Loading....");
    var callUrl = $("#webUrl").val() + "/StorageDrive/ListAll";
    $.ajax({
        url: callUrl,
        type: "POST",
        data: dataToSend,
        cache: false,
        success: function (htmlContent) {
            $("#divGetFolderList").html(htmlContent);
            if (!isRefreshCurrent) {
                LoadBreadcrumb(folderName, folderId);
            }
        },
        error: function (msg) {
            SetValidationErrorMessage(msg);
        }
    });
}

function DownloadFile(fileLink) {
    window.open(fileLink, '_blank');
}

function DeleteFile(fileId, sender) {
    var confirmBoxResult = confirm("Are you sure want to delete this file ?");
    if (confirmBoxResult) {
        var dataToSend = { fileId: fileId, type: $("#hdnCurrentSelectedType").val() };
        var callUrl = $("#webUrl").val() + "/StorageDrive/DeleteFile";
        $.ajax({
            url: callUrl,
            type: "POST",
            data: dataToSend,
            cache: false,
            success: function (result) {
                if (result.Success) {
                    alert("File Deleted successfully.");
                    $(sender).closest('.list-group-item').remove();
                }
                else {
                    alert("Something went wrong.");
                }
            },
            error: function (msg) {
                alert("Something went wrong.");
            }
        });
    }
}

function OnFileNameClick(sender) {
    var form = $(sender).closest('.updateElementClass');
    $("#FileNameLabel", form).css("display", "none");
    $("#FileNameInput", form).css("display", "block");
}

function OnKeyUpRename(sender, fileId) {
    var form = $(sender).closest('.updateElementClass');
    if (event.keyCode == 13) {
        var dataToSend = { fileId: fileId, newName: sender.value, type: $("#hdnCurrentSelectedType").val() };
        var callUrl = $("#webUrl").val() + "/StorageDrive/RenameFileName";
        $.ajax({
            url: callUrl,
            type: "POST",
            data: dataToSend,
            cache: false,
            success: function (result) {
                if (result.Success) {
                    $("#FileNameLabel", form).css("display", "block");
                    $("#FileNameInput", form).css("display", "none");
                    $("#FileNameLabel", form).html(sender.value);
                }
                else {
                    alert("Something went wrong.");
                }
            },
            error: function (msg) {
                alert("Something went wrong.");
            }
        });
    }
}

function OnFileUpload(sender) {
    var fileUpload = $("#FileUpload1").get(0);
    var files = fileUpload.files;

    // Create FormData object  
    var fileData = new FormData();

    // Looping over all files and add it to FormData object  
    for (var i = 0; i < files.length; i++) {
        fileData.append(files[i].name, files[i]);
    }

    // Adding one more key to FormData object  
    fileData.append('Parent', $("#hdnCurrentParent").val());
    fileData.append('Type', $("#hdnCurrentSelectedType").val());

    $.ajax({
        url: '/StorageDrive/UploadFiles',
        type: "POST",
        contentType: false, // Not to set any content header  
        processData: false, // Not to process data  
        data: fileData,
        success: function (result) {
            GetList($("#hdnFolderName").val(), $("#hdnCurrentParent").val(), true);
        },
        error: function (err) {
            alert(err.statusText);
        }
    });
}

function LoadBreadcrumb(folderName, folder) {
    var itemCount = $("#storage-breadcrumb").find("li").length;
    if (folderName == '') {
        $("#storage-breadcrumb").html('');
        $("#storage-breadcrumb").append('<li><a style="cursor: pointer;" onclick="ReverseLoadBreadcrumb(0,\'\',\'\')">Home</a></li>');
    }
    else {
        $("#storage-breadcrumb").append('<li><a style="cursor: pointer;" onclick="ReverseLoadBreadcrumb(' + itemCount + ',\'' + folderName + '\',\'' + folder + '\')">' + folderName + '</a></li>');
    }
}

function ReverseLoadBreadcrumb(position, folderName, folder) {
    if (folderName == "") {
        GetList("", "", false);
    }
    else {
        var breadCrumbItemList = $("#storage-breadcrumb").find("li");
        for (var i = 0; i < breadCrumbItemList.length; i++) {
            if (i >= position)
                $(breadCrumbItemList[i]).remove();
        }
        GetList(folderName, folder, false);
    }
}