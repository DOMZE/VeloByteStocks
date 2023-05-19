# About
This is the API that is used in my blog post [Load testing your applications using Azure Load Testing, JMeter and GitHub actions](https://www.domstamand.com/load-testing-your-applications-using-azure-load-testing-jmeter-and-github-actions/).

The infra code is heavily inspired (and at times borrowed) from the repository of Azure-Samples [app-templates-dotnet-azuresql-appservice](https://github.com/Azure-Samples/app-templates-dotnet-azuresql-appservice/tree/main).

# JMeter
Generating the CSV from the Database data can be done using the following SQL Query
```sql
WITH DataSet (Ticker, /*MinDate, MaxDate, DaysDiff,*/ RandomDateBetweenMinMax) AS
(
	SELECT Tickers.Ticker,
	/*-- The 3 commented lines are for debug purpose
	MIN(TickersHistory.[Date]) AS MinDate,
	--MAX(TickersHistory.[Date]) AS MaxDate,
	--DATEDIFF(day ,MIN(TickersHistory.[Date]), MAX(TickersHistory.[Date])) AS DaysDiff,*/
	DATEADD(day, DATEDIFF(day ,MIN(TickersHistory.[Date]), MAX(TickersHistory.[Date])) / CAST((RAND() * (10 - 2)) + 2 AS INT) /* Rand(2,10)*/, MIN(TickersHistory.[Date])) AS RandomDateBetweenMinMax
	FROM Tickers
	INNER JOIN TickersHistory ON Tickers.Id = TickersHistory.TickerId
	GROUP BY Tickers.Ticker
)
SELECT Ticker,
	   RandomDateBetweenMinMax AS StartDate,
	   DATEADD(day, CAST((RAND() * (30 - 1)) + 1 AS INT), RandomDateBetweenMinMax) /* Rand(1,30)*/ AS EndDate
FROM DataSet
ORDER BY Ticker
```

# Data
The data set comes from Kaggle and uses Yahoo Finance All Stocks Dataset. You can find the dataset [here](https://www.kaggle.com/datasets/tanavbajaj/yahoo-finance-all-stocks-dataset-daily-update).
<br/>You can find a snapshot of the data in the data directory.
<br/>Use the VeloByte.DataLoader project to load the data into your database.
<br/>Example:
```shell
dotnet run -c Release src/VeloByte.DataLoader/VeloByte.DataLoader.csproj -- -c "<connection_string>" -d "<path_to_csv_files>" -s "<path_to_schema_file>"
```

# Azure provisioning
## Managed Identity

If you wish to enable the `UseManageIdentity` option on the webapp, you will need to add the app service as a user into the database.
Since only connections established with Active Directory accounts can create other Active Directory users, you will need to connect to the server and execute the following command in the created database.
You can follow the [documentation](https://learn.microsoft.com/en-us/azure/app-service/tutorial-connect-msi-sql-database?tabs=windowsclient%2Cef%2Cdotnet#grant-permissions-to-managed-identity) on how to do this.

For the permissions, you can add the app service into the db_owner role for easiness.
```sql
CREATE USER [<APP_SERVICE_NAME>] FROM EXTERNAL PROVIDER;
GO
ALTER ROLE db_owner ADD MEMBER [<APP_SERVICE_NAME>];
GO
```

## Authentication
The authentication is done through EasyAuth and not within the code for simplicity. Refer to the bicep code to see what is needed.

## Azure AD Application for authentication
The API is protected by an Azure AD application. You can create one and enable ROPC flow by enabling the **Allow public client flows** setting in the Authentication blade.
<br/>Note that how to exclude users from MFA (using conditional access policies) is not part of the post or this repository.

## App Deployment
You can run the following commands to deploy your app to Azure
```powershell
dotnet publish -c Release .\src\VeloByte.StocksAPI\VeloByte.StocksAPI.csproj
Compress-Archive -Path .\src\VeloByte.StocksAPI\bin\Release\net7.0\publish\* -DestinationPath <destination_path>\VeloByte.API.zip
Publish-AzWebApp -ResourceGroupName <your_resourcegroup_name> -Name <your_web_app_name> -ArchivePath <destination_path>\VeloByte.API.zip -Force
```