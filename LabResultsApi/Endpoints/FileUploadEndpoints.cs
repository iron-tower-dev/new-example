using Microsoft.AspNetCore.Mvc;
using LabResultsApi.Services;
using LabResultsApi.DTOs;

namespace LabResultsApi.Endpoints;

public static class FileUploadEndpoints
{
    public static void MapFileUploadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/files")
            .WithTags("File Upload")
            .WithOpenApi();

        group.MapPost("/upload", UploadFile)
            .WithName("UploadFile")
            .WithSummary("Upload a file for a specific sample and test")
            .DisableAntiforgery(); // Required for file uploads

        group.MapGet("/sample/{sampleId:int}/test/{testId:int}", GetFiles)
            .WithName("GetFiles")
            .WithSummary("Get files for a specific sample and test");

        group.MapGet("/sample/{sampleId:int}/test/{testId:int}/trial/{trialNumber:int}", GetFilesByTrial)
            .WithName("GetFilesByTrial")
            .WithSummary("Get files for a specific sample, test, and trial");

        group.MapGet("/{fileId:int}", GetFile)
            .WithName("GetFile")
            .WithSummary("Get file information by ID");

        group.MapGet("/{fileId:int}/download", DownloadFile)
            .WithName("DownloadFile")
            .WithSummary("Download file content by ID");

        group.MapDelete("/{fileId:int}", DeleteFile)
            .WithName("DeleteFile")
            .WithSummary("Delete a file by ID");

        group.MapGet("/sample/{sampleId:int}/test/{testId:int}/preview", GetFilePreviews)
            .WithName("GetFilePreviews")
            .WithSummary("Get file preview information for a sample and test");

        group.MapGet("/test/{testId:int}/supported-extensions", GetSupportedExtensions)
            .WithName("GetSupportedExtensions")
            .WithSummary("Get supported file extensions for a test type");

        group.MapPost("/validate", ValidateFile)
            .WithName("ValidateFile")
            .WithSummary("Validate a file before upload")
            .DisableAntiforgery();
    }

    private static async Task<IResult> UploadFile(
        HttpRequest request,
        IFileUploadService fileUploadService)
    {
        try
        {
            // Check if request contains files
            if (!request.HasFormContentType || !request.Form.Files.Any())
            {
                return Results.BadRequest(new FileUploadResponseDto
                {
                    Success = false,
                    Message = "No file provided",
                    Errors = new List<string> { "Request must contain a file" }
                });
            }

            var file = request.Form.Files[0];

            // Parse form data for upload request
            var uploadRequest = new FileUploadRequestDto();
            
            if (int.TryParse(request.Form["sampleId"], out var sampleId))
                uploadRequest.SampleId = sampleId;
            else
                return Results.BadRequest(new FileUploadResponseDto
                {
                    Success = false,
                    Message = "Invalid sample ID",
                    Errors = new List<string> { "Sample ID is required and must be a valid integer" }
                });

            if (int.TryParse(request.Form["testId"], out var testId))
                uploadRequest.TestId = testId;
            else
                return Results.BadRequest(new FileUploadResponseDto
                {
                    Success = false,
                    Message = "Invalid test ID",
                    Errors = new List<string> { "Test ID is required and must be a valid integer" }
                });

            if (int.TryParse(request.Form["trialNumber"], out var trialNumber))
                uploadRequest.TrialNumber = trialNumber;
            else
                return Results.BadRequest(new FileUploadResponseDto
                {
                    Success = false,
                    Message = "Invalid trial number",
                    Errors = new List<string> { "Trial number is required and must be a valid integer" }
                });

            uploadRequest.Description = request.Form["description"].ToString() ?? "";

            // For now, use a default user ID - in production this would come from authentication
            var uploadedBy = request.Form["uploadedBy"].ToString() ?? "System";

            var result = await fileUploadService.UploadFileAsync(file, uploadRequest, uploadedBy);
            
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error uploading file: {ex.Message}");
        }
    }

    private static async Task<IResult> GetFiles(
        int sampleId, 
        int testId, 
        IFileUploadService fileUploadService)
    {
        try
        {
            var files = await fileUploadService.GetFilesAsync(sampleId, testId);
            return Results.Ok(files);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving files for sample {sampleId}, test {testId}: {ex.Message}");
        }
    }

    private static async Task<IResult> GetFilesByTrial(
        int sampleId, 
        int testId, 
        int trialNumber, 
        IFileUploadService fileUploadService)
    {
        try
        {
            var files = await fileUploadService.GetFilesAsync(sampleId, testId, trialNumber);
            return Results.Ok(files);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving files for sample {sampleId}, test {testId}, trial {trialNumber}: {ex.Message}");
        }
    }

    private static async Task<IResult> GetFile(int fileId, IFileUploadService fileUploadService)
    {
        try
        {
            var file = await fileUploadService.GetFileAsync(fileId);
            if (file == null)
            {
                return Results.NotFound($"File {fileId} not found");
            }
            return Results.Ok(file);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving file {fileId}: {ex.Message}");
        }
    }

    private static async Task<IResult> DownloadFile(int fileId, IFileUploadService fileUploadService)
    {
        try
        {
            var fileData = await fileUploadService.DownloadFileAsync(fileId);
            if (fileData == null)
            {
                return Results.NotFound($"File {fileId} not found or cannot be downloaded");
            }

            var (content, fileName, contentType) = fileData.Value;
            return Results.File(content, contentType, fileName);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error downloading file {fileId}: {ex.Message}");
        }
    }

    private static async Task<IResult> DeleteFile(
        int fileId, 
        IFileUploadService fileUploadService,
        [FromQuery] string? deletedBy = null)
    {
        try
        {
            var deleted = await fileUploadService.DeleteFileAsync(fileId, deletedBy ?? "System");
            if (!deleted)
            {
                return Results.NotFound($"File {fileId} not found or could not be deleted");
            }
            return Results.Ok(new { Success = true, Message = "File deleted successfully" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error deleting file {fileId}: {ex.Message}");
        }
    }

    private static async Task<IResult> GetFilePreviews(
        int sampleId, 
        int testId, 
        IFileUploadService fileUploadService)
    {
        try
        {
            var previews = await fileUploadService.GetFilePreviewsAsync(sampleId, testId);
            return Results.Ok(previews);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving file previews for sample {sampleId}, test {testId}: {ex.Message}");
        }
    }

    private static async Task<IResult> GetSupportedExtensions(
        int testId, 
        IFileUploadService fileUploadService)
    {
        try
        {
            var extensions = await fileUploadService.GetSupportedExtensionsAsync(testId);
            return Results.Ok(new { TestId = testId, SupportedExtensions = extensions });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving supported extensions for test {testId}: {ex.Message}");
        }
    }

    private static async Task<IResult> ValidateFile(
        HttpRequest request,
        IFileUploadService fileUploadService)
    {
        try
        {
            if (!request.HasFormContentType || !request.Form.Files.Any())
            {
                return Results.BadRequest(new { IsValid = false, Errors = new[] { "No file provided" } });
            }

            var file = request.Form.Files[0];

            var uploadRequest = new FileUploadRequestDto();
            
            if (int.TryParse(request.Form["sampleId"], out var sampleId))
                uploadRequest.SampleId = sampleId;
            else
                return Results.BadRequest(new { IsValid = false, Errors = new[] { "Invalid sample ID" } });

            if (int.TryParse(request.Form["testId"], out var testId))
                uploadRequest.TestId = testId;
            else
                return Results.BadRequest(new { IsValid = false, Errors = new[] { "Invalid test ID" } });

            if (int.TryParse(request.Form["trialNumber"], out var trialNumber))
                uploadRequest.TrialNumber = trialNumber;
            else
                return Results.BadRequest(new { IsValid = false, Errors = new[] { "Invalid trial number" } });

            var (isValid, errors) = await fileUploadService.ValidateFileAsync(file, uploadRequest);
            
            return Results.Ok(new { IsValid = isValid, Errors = errors });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error validating file: {ex.Message}");
        }
    }
}