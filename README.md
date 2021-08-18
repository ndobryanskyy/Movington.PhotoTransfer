# Movington | Photo Transfer

High performance and low-allocations tool, based on [TPL Dataflow](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library) for transferring personal photos from OneDrive to Google Photos.

### Setup

* Register Azure AD application for "All Microsoft account users". Allow `Files.Read.All` scope. Specify client configuration under `Authentication:OneDrive` section (check `OneDriveAuthenticationOptions` class for required properties)
* Register Google Cloud client app. Enable Google Photos API for its containing Google Cloud project. Allow `https://www.googleapis.com/auth/photoslibrary.appendonly` scope. Specify client configuration under `Authentication:GooglePhotos` section (check `GoogleAuthenticationOptions` class for required properties).
* Specify pipeline options under `TransferPipeline` section (check `TransferPipelineOptions` class for properties).

### Running

After setting up all required options, just run the app in `Release` mode. App will log only `Warning` level messages in the console. All detailed log messages could be found in the file named after date/time of start `~/Documents/.movington/Logs` (logs are written in `Serilog` compact format, structured logging is used).



