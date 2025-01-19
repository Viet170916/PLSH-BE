using System.Diagnostics.CodeAnalysis;

namespace Common.Library
{
  [ExcludeFromCodeCoverage]
  public static class Constants
  {
    public static readonly string CorsPolicy = "CorsPolicy";
    public static readonly string Issuer = "https://localhost:5000";
    public static readonly string Audience = "https://localhost:3000";
    public static readonly string JWT_SECRET = "JWT_SECRET";
    public static readonly string SwaggerEndpointName = "Kpmg BRP API";
    public static readonly string SwaggerEndpointUrl = "/swagger/v1/swagger.json";
    public static readonly string ApiVersion = "v1";
    public static readonly string ApiTitle = "BRP API";
    public static readonly string ConnStr = "DefaultConnection";
    public static readonly string JwtDescription = "JWT Authorization header using the Bearer scheme.";
    public static readonly string Bearer = "Bearer";
    public static readonly string Authorization = "Authorization";
    public static readonly string Jwt = "JWT";
    public static readonly string ResponseHeader = "Access-Control-Allow-Origin";
    public static readonly string ApplicationError = "Application-Error";
    public static readonly string AccessControlExposeHeaders = "access-control-expose-headers";
    public static readonly string Star = "*";
    public static readonly string AspnetcoreEnvironment = "ASPNETCORE_ENVIRONMENT";
    public static readonly string Development = "Development";
    public static readonly string Production = "Production";
    public static readonly string Success = "Success";
    public static readonly string Failed = "Failed";
    public static readonly string System = "System";
    public static readonly string Yes = "Yes";
    public static readonly string No = "No";
    public static readonly string Single = "Single";
    public static readonly string Error = "Error";
    public static readonly string UserAlreadyExists = "User already exists!";
    public static readonly string UserCreatedSuccessfully = "User created successfully!";
    public static readonly string ApplicationJson = "application/json";
    public static readonly string Json = ".json";
    public static readonly string Minus = " - ";
    public static readonly string FormatDate = "dd/MM/yyyy";
    public static readonly string FormatDateExport = "dd-MMM-yy";
    public static readonly string ApplicationPdf = "application/pdf";

    public static readonly string
      ApplicationExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static readonly string GetFileSuccess = "Get File Success";
    public static readonly string ConvertBase64 = "data:application/pdf;base64 ";
    public static readonly string LineBreak = "<br/>";
    public static readonly string Permissions = "Permissions";
    public static readonly string NoResultMatched = "No Result Matched !";
    public static readonly string SearchDefaulStringValue = "-1";
    public static readonly string StatusInactive = "0";
    public static readonly string AuditFuntion = "Audit";
    public static readonly string NonAuditFuntion = "Non-Audit";
    public static readonly string ToCurrency = "SGD";
    public static readonly int OneHundred = 100;
    public static readonly string Delimiter = ", ";
    public static readonly string StatusActive = "1";
    public static readonly string Results = "Results";
    public static readonly string N = "N";
    public static readonly string Y = "Y";
    public static readonly string FormatDateEmail = "dd MMM yyyy";
    public static readonly string FormatDateTimeEmail = "dd MMM yyyy hh:mm";
    public static readonly string Colon = ":";
    public static readonly string Slash = "/";
    public static readonly string Comma = ",";
    public static readonly string Space = " ";
    public static readonly string UsedByAnotherUser = "Used by another user";
    public static readonly string Separator = ";";
    public static readonly string SystemUser = "System User";
    public static readonly string BatchUser = "Batch User";

    public static class ErrorMessages
    {
      public static readonly string ModelStateMessage = "Model State Invalid";
      public static readonly string SuccessMessage = "Successfully";
      public static readonly string NoRecordsFoundMessage = "No records found";
      public static readonly string ValidationFailedMessage = "Validations Failed.";
      public static readonly string RequestInvalid = "Request Invalid.";
      public static readonly string ModelIsInvalidMessage = "Model is invalid.";

      public static readonly string GeneralExceptionMessage =
        "Some error occurred. Please contact your system administrator.";

      public static readonly string InvalidUserNameMessage = "Invalid Username or Password.";
      public static readonly string UnauthorizedUserMessage = "Unauthorized User.";
      public static readonly string RoleNotExistedMessage = "Role Not Found.";
      public static readonly string UserNotFoundMessage = "User Not Found";
      public static readonly string InternalServerErrorMessage = "Internal Server Error.";
      public static readonly string GetDataFailedPleaseTryAgain = "Get data failed. Please try again.";
      public static readonly string EmptyTemplate = "Template is empty";
      public static readonly string TemplateHasNoSignatureField = "Template contains no signature field.";
      public static readonly string NoSignature = "User has no signature.";
      public static readonly string NoFile = " No file uploaded.";
      public static readonly string ImageSizeExceedLimit = "Image exceeds the maximum resolution allowed (1920 x 1080)";
      public static readonly string FileNotSupported = "The file format is not supported.";
      public static readonly string Unauthorized = "Unauthorized.";
      public static readonly string FileNotFound = "File Not Found";
      public static readonly string NoFileDownload = "No file download.";
      public static readonly string LimitedLength = "Limited length";
      public static readonly string InvalidRole = "Invalid Role";
      public static readonly string EmptyRecipient = "Empty recipient";
      public static readonly string Over2MB = "The uploaded file is exceeding maximum size (2M)";
      public static readonly string IndexSelected = "The Index is already selected";
      public static readonly string BadParam = "Param can not null or empty";
      public static readonly string AddFail = "Can not add data";
      public static readonly string BudgetGroupNotExisted = "The budget group not existed";
      public static readonly string UpdateFail = "Can not update data";
      public static readonly string DeleteFail = "Can not delete data";
      public static readonly string SubGroupExisted = "The Subgroup already exists";
      public static readonly string TaggingEntityExisted = "The Tagging Entity already exists";

      public static readonly string TaggingEntityExistedPrimary =
        "The Tagging Entity and engagement code already exist";

      public static readonly string SubconCostIsNotExisted = "The Sub Contract Cost is not exists";
      public static readonly string NonBillableExpenseNotExisted = "The Non Billable Expense is not exists";
      public static readonly string NotHavePermission = "Not Have Permission To Access";
      public static readonly string EmployeeExisted = "The Employee already exists";
      public static readonly string TargetChargeableHoursIsNotExisted = "The target chargeable hours is not exists";
      public static readonly string FyNotNull = "Fy can not null";
    }

    public static class UrlConst
    {
      public static readonly string GetBookingById = "api/booking/id?id=";
      public static readonly string GetAllByListId = "api/sub-group/get-all-by-list-sub-id";
      public static readonly string GetBookingAndBookingRequest = "api/booking/booking-and-booking-request";
      public static readonly string GetBookingRequestDailyFactor = "api/booking/booking-request-daily-factor";
      public static readonly string GetBookingRequestForEng = "api/booking/booking-request-for-eng-hour";

      public static readonly string GetBookingRequestDailyFactorOverview =
        "api/booking/booking-request-daily-factor-overview";

      public static readonly string GetAdjustmentToStaffHoursofBookingRequest =
        "api/booking/adjustment-staff-hour-of-booking-request";

      public static readonly string GetSubgroupBySearch =
        "api/sub-group/get-subgroup-information-by-search?SubgroupName={0}&Month={1}&Year={2}&TypeOfWork={3}" +
        "&ClientGroupId={4}&SubGroupId={5}&YearEnd={6}";

      public static readonly string GetSubgroupInfo = "api/sub-group/get-subgroup-info-by-client-id?clientNo=";
      public static readonly string GetNotWorkingDay = "api/booking/calculate-not-working-day";
      public static readonly string GetProfitCenterByBu = "api/common/get-profit-center-by-code?buNo=";
      public static readonly string AddSubgroupCrs = " api/sub-group/add-subgroup";
      public static readonly string GetAllKeyDeadLineType = "api/key-deadline/get-all-key-deadline-type/";
      public static readonly string AddKeyDeadline = " api/key-deadline/add-key-deadline";
      public static readonly string UpdateKeyDeadline = "api/key-deadline/update-key-deadline";
      public static readonly string DeleteKeyDeadline = "api/key-deadline/delete-key-deadline";
      public static readonly string SearchSubGroupByName = "api/sub-group/search-subgroup-by-name";
      public static readonly string GetListSubgroupByName = "api/sub-group/get-list-subgroup-information-by-search";
      public static readonly string GetKeyDeadlineSubgroup = "api/key-deadline/get-key-deadline-subgroup";
      public static readonly string GetSubgroupInforByIds = "api/sub-group/get-subgroup-info-by-ids";
      public static readonly string GetNotWorkingDayUpdate = "api/booking/calculate-not-working-day-update";
      public static readonly string GetNotWorkingDayByFY = "api/booking/calculate-not-working-day-by-kpmgfy";
      public static readonly string GetBookingRequestDailyFactorByFY = "api/booking/booking-request-daily-factor-by-fY";
      public static readonly string GetCrsAdmin = "api/crs-employee/get-crs-admin-so";
      public static readonly string CheckDeleteSubGroup = "api/sub-group/check-delete-subgroup?subGroupId=";
      public static readonly string GetBuCooByBuNo = "api/common/get-bucoo-by-bu-no";
      public static readonly string GetListGrade = "api/common/get-list-grade";

      public static readonly string GetKeyDeadlineSubgroupByBudget =
        "api/key-deadline/get-key-deadline-subgroup-by-budget";

      public static readonly string UpdateSubgroupCrs = " api/sub-group/update-subgroup";
      public static readonly string GetSubInfo = "api/sub-group/get-all-sub-info";
    }

    public static class SessionConstants
    {
      public static readonly string CurrentUserRole = "CurrentUserRole";
    }

    public static class Strings
    {
      public static class JwtClaimIdentifiers
      {
        public static readonly string Rol = "rol", Id = "id";
      }

      public static class RoleIdentifiers
      {
        public static readonly string Normal = "Normal";
        public static readonly string Administrator = "Administrator";
      }

      public static class JwtClaims
      {
        public static readonly string ApiAccess = "api_access";
      }
    }

    public static class SpecialListTypeConst
    {
      public static readonly string TAX = "TAX";
      public static readonly string TaxOthers = "Tax-others";
      public static readonly string TaxCode = "TAX";
      public static readonly string TaxOthersCode = "TO";
      public static readonly string IRM = "IRM";
      public static readonly string Valuation = "Valuation";
      public static readonly string DataAnalytics = "Data Analytics";
      public static readonly int ValueDefaultRateCYBudget = 40;
      public static readonly int ValueDefaultRateCYBudget2 = 37;
    }

    public static class SettingKeyConst
    {
      public static readonly string TAX_Function_Default_CC_For_Costing = "TAX_Function_Default_CC_For_Costing";
    }

    public static class ColumnFilterBudget
    {
      public static readonly string Status = "Status";
      public static readonly string Budgetgroup = "Budgetgroup";
      public static readonly string TypeOfWork = "TypeOfWork";
      public static readonly string Recurring = "Recurring";
      public static readonly string YearEnd = "YearEnd";
      public static readonly string ClientGroupName = "ClientGroupName";
      public static readonly string PIE = "PIE";
      public static readonly string Exempt = "Exempt";
      public static readonly string NewClient = "NewClient";
      public static readonly string LastYearOfAudit = "LastYearOfAudit";
      public static readonly string BU = "BU";
      public static readonly string EP = "EP";
      public static readonly string EM = "EM";
      public static readonly string CoEMs = "CoEMs";
      public static readonly string EQCR = "EQCR";
      public static readonly string EPIds = "EPIds";
      public static readonly string EMIds = "EMIds";
      public static readonly string CoEMsIds = "CoEMsIds";
      public static readonly string EQCRIds = "EQCRIds";
      public static readonly string ApprovalDate = "ApprovalDate";
      public static readonly string LastModifiedBy = "LastModifiedBy";
      public static readonly string LastModifiedDate = "LastModifiedDate";
      public static readonly string KPMGFY = "KPMGFY";
      public static readonly string APICBudget = "APICBudget";
    }

    public static class ColumnSortBudget
    {
      public static readonly string Status = "budgetGroupStatus.name";
      public static readonly string Budgetgroup = "budgetGroupName";
      public static readonly string TypeOfWork = "typeOfWork.name";
      public static readonly string Recurring = "recurring";
      public static readonly string YearEnd = "yearEnd";
      public static readonly string ClientGroupName = "clientGroupName";
      public static readonly string PIE = "pie";
      public static readonly string Exempt = "exempt.exemptName";
      public static readonly string NewClient = "newClient";
      public static readonly string LastYearOfAudit = "lastYearOfAudit";
      public static readonly string BU = "bu";
      public static readonly string EP = "ep";
      public static readonly string EM = "em";
      public static readonly string CoEMs = "coeMs";
      public static readonly string EQCR = "eqcr.name";
      public static readonly string ApprovalDate = "apicApprovalDate";
      public static readonly string LastModifiedBy = "lastUpdatedBy.employeeName";
      public static readonly string LastModifiedDate = "lastUpdatedDate";
      public static readonly string KPMGFY = "fy";
      public static readonly string APICBudget = "statusAPIC.name";
    }

    public static class SortType
    {
      public static readonly string Desc = "desc";
      public static readonly string Asc = "asc";
    }

    public static class GradeString
    {
      public static readonly string Grades = ", P, PA1, D1S, AM, M, D1, D2, PA2, S3, S4, G1, G2, A3, A2, A1";
    }

    public static class ChargeOutRateConst
    {
      public static readonly string Advisory = "Advisory";
      public static readonly string Tax = "Tax";
      public static readonly string Audit = "Audit";
    }

    public static class Log
    {
      public static readonly string StartAuthenication = "Start Authenication";
      public static readonly string BRPHostStart = "BRP host started ...";
      public static readonly string BRpHostTerminated = "BRP host terminated unexpectedly.";
      public static readonly string BRpHostClosed = "BRP host closed.";
      public static readonly string StartGetUser = "Start GetUserRoles";
      public static readonly string UserCalled = "get user role called";
      public static readonly string AddUserCalled = "add user role called";
      public static readonly string RemoveUserCalled = "remove user role called";
      public static readonly string EmployeeCalled = "get employee called";
      public static readonly string RoleCaled = "get role called";
      public static readonly string SendEmailRemindPendingBGForEP = $"SendEmailRemindPendingBGForEP - 3";
    }

    public static class StartUp
    {
      public static readonly int TimeSpanHours = 24;
      public static readonly int TimeSpanDays = 30;
    }

    public static class SearchEmployeeType
    {
      public static readonly string EQCR = "EQCR";
      public static readonly string EP = "EP";
      public static readonly string COEMS = "COEMS";
      public static readonly string EM = "EM";
      public static readonly string SO = "SO";
    }

    public static class TypeUpdate
    {
      public static readonly string Apic = "APIC";
      public static readonly string BU = "BU";
      public static readonly string Portfolio = "Portfolio";
    }

    public static class CustomMessage
    {
      public static readonly string MSG11 = "Specialist Hours added successfully";
      public static readonly string MSG12 = "Specialist Hours updated successfully";
      public static readonly string MSG18 = "This Specialist type already exists";
      public static readonly string MSG18dot1 = "This subgroup is already tagged to an existing Budget Group";

      public static readonly string AddTagingSubgroupSuccess =
        "Subgroup <b class = 'red_color'>{0}</b> tagged successfully";

      public static readonly string MSG21 = "This Budget Group has been locked by APIC.";
      public static readonly string MSG23 = "Budget group updated successfully";

      public static readonly string MSG37 =
        "This Subgroup is the Primary Subgroup of another Budget Group. Please select a different Subgroup.";

      public static readonly string MSG5 = "The budget group already exists";
      public static readonly string AddBudgetGroupSuccess = "New record created successfully";
      public static readonly string MSG42 = "This Subcontractor Cost already exists";
      public static readonly string MSG35 = "Subcontractor Costs added successfully";
      public static readonly string MSG34 = "Subcontractor Costs updated successfully";
      public static readonly string MSG46 = "The Primary Entity is PIE.You can't de-select PIE for this Budget Group.";
      public static readonly string MSG38 = "This Billing Schedule already exists";
      public static readonly string MSG30 = "Billing Schedule added successfully";
      public static readonly string MSG31 = "Billing Schedule updated successfully";
      public static readonly string MSG61 = "Total Finalized Fee updated successfully";
      public static readonly string MSG58 = "Budget Group(s) submitted successfully.";
      public static readonly string MSG58S = "Budget Group(s) returned successfully.";
      public static readonly string MSG62 = "You cannot approve Budget Groups that are not “Pending Approval”";
      public static readonly string MSG62s = "You cannot reject Budget Groups that are not “Pending Approval”";
      public static readonly string MSG66 = "Please select a valid Budget Group to submit to APIC";
      public static readonly string MSG66s = "Budget groups Submit To APIC successfully";

      public static readonly string MSG67 =
        "You cannot approve Budget Groups whose APIC status is different from “Submitted”";

      public static readonly string MSG67S =
        "You cannot return Budget Groups whose APIC status is different from “Submitted”";

      public static readonly string MSG56 = "Submit Successfully";
      public static readonly string MSG53 = "Please select at least one Budget Group";
      public static readonly string MSG63 = "Please select at least one Budget Group to approve";
      public static readonly string MSG57 = "Budget groups Approved successfully";
      public static readonly string MSG57s = "Budget groups Reject successfully";
      public static readonly string MSG68 = "Budget Group has been accepted";
      public static readonly string MSG49 = "New resource added successfully";

      public static readonly string MSG59 =
        "</br>Audit CY Budget updated successfully. The Budget Group will be submitted to EP to re-approve " +
        "because the CY Budget Engagement Hours has been increased by more than 5%";

      public static readonly string MSG15 = "Audit CY Budget updated successfully";
      public static readonly string NotUpdate = "Can not update budget group with Pending Approval status";
      public static readonly string MSG59dot1 = "Updated Budget Group has been submitted to EP to re-approve";
      public static readonly string MSG76 = "Only Budget Group drafts can be deleted.";
      public static readonly string MSG75 = "Please select at least one Budget Group";

      public static readonly string MSG78 =
        "Primary Entity of this Budget Group was deleted in source system. Please replace with new Primary Entity";

      public static readonly string MSG79 =
        "Primary Subgroup of this Budget Group was deleted in CRS. Please replace with new Primary Subgroup";

      public static readonly string MSG69 = "Are you sure you want to delete?";

      public static readonly string MSG80 =
        "This Budget Group is deleted by APIC. Please refresh the page to contitnue.";

      public static readonly string InvalidState = "Invalid State";
      public static readonly string ExpiredBGToSubmit = "Expired BG to submit";
      public static readonly string NoTaggedEntity = "No Tagged Entity";
      public static readonly string NoTaggedSubgroup = "No tagged Subgroup";
      public static readonly string MissingCYBudgetManagement = "Missing CY Budget Management";
      public static readonly string MissingAPIC = "Last year of audit ( to exclude from APIC Budget) is required";
      public static readonly string MissingAccountingStandard = "Missing Accounting Standard";
      public static readonly string MissingAudittingStandard = "Missing Auditting Standard";
      public static readonly string MissingGrossFee = "Missing Gross Fee";
      public static readonly string MissingBillingSchedule = "Missing Billing Schedule";
      public static readonly string UnlockedBudgetGroup = "Budget group is not locked";
      public static readonly string LockedBudgetGroup = "Budget group is already locked";
      public static readonly string BudgetGroupDraftsCannotSubmitted = "Drafts cannot be submitted";

      public static readonly string SubmitToAPICStatusError =
        "Budget Group is not Approved and BG APIC status is not blank or Returned";

      public static readonly string ApproveOrRejectBudgetGroupStatusError = "Budget Group is not Pending Approval";
      public static readonly string AcceptOrReturnBudgetGroupStatusError = "Budget Group APIC status is not Submitted";

      public static readonly string BudgetLocked =
        "Budget Group locked. {0}({1}) is currently editing the Budget Group.";

      public static readonly string WrongStatus = "Budget Group cannot be edited during Pending Approval";
    }

    public static class TemplateFile
    {
      public static readonly string TEMPLATE_MY_PORTFOLIO = "Templates/TEMPLATE_MY_PORTFOLIO.xlsx";
      public static readonly string TEMPLATE_TAGGING_ENTITIES = "Templates/TEMPLATE_TAGGING_ENTITIES.xlsx";
      public static readonly string TEMPLATE_TAGGING_ENTITIES_REPONSE = "TEMPLATE_TAGGING_ENTITIES.xlsx";
      public static readonly string TEMPLATE_MY_PORTFOLIO_REPONSE = "TEMPLATE_MY_PORTFOLIO.xlsx";
      public static readonly string TEMPLATE_TAGGING_ENTITIES_RESPONSE = "TEMPLATE_TAGGING_ENTITIES.xlsx";
      public static readonly string TEMPLATE_MY_PORTFOLIO_RESPONSE = "TEMPLATE_MY_PORTFOLIO.xlsx";
      public static readonly string SheetFile = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
      public static readonly string MyPortfolioFile = "MY_PORTFOLIO";
      public static readonly string TaggingEntityFile = "ENTITIES_ENGT_LIST";
      public static readonly string BillingScheduleFile = "BILLING SCHEDULE";
      public static readonly string FileExtend = ".xls";
      public static readonly string FileExtendXLSX = ".xlsx";
      public static readonly string Hyphen = "_";
      public static readonly string TEMPLATE_BILLING_SCHEDULE = "Templates/Template_Billing_Schedule.xlsx";
      public static readonly string TEMPLATE_BILLING_SCHEDULE_RESPONSE = "TEMPLATE_BILLING_SCHEDULE.xlsx";
      public static readonly string ZipArchival = " ARCHIVAL_DATA_BRP_{0}_{1}.zip";
      public static readonly string FileExtendCsv = ".csv";
      public static readonly string WorkloadAssessmentOverview = "WORKLOAD_ASSESSMENT_OVERVIEW";
      public static readonly string WorkloadAssessmentOverviewByMonth = "WORKLOAD_ASSESSMENT_OVERVIEW_BY_MONTH";
      public static readonly string WorkloadAssessmentOverviewSheetName = "WORKLOAD ASSESSMENT BY MONTH";
      public static readonly string BudgetOverview = "BUDGET GROUP OVERVIEW";
      public static readonly string BudgetGroupForBUFile = "Draft_Reject_Engagement_Budgets";
      public static readonly string AuditTrails = "AUDIT_TRAILS";
      public static readonly string TaggingEntitiesEngtCodes = "Tagging Entities & Engt Codes";
      public static readonly string ExportForecastBudgetReport = "Export Forecast & Budget report";
    }

    public static class EntityName
    {
      public static readonly string BudgetGroups = "Budget Groups";
      public static readonly string BudgetGroupsCoEMs = "Budget Groups CoEMs";
      public static readonly string TaggingSubgroups = "Tagging Subgroups";
      public static readonly string TaggingEntities = "Tagging Entities";
      public static readonly string CYBudgetAuditHours = "CY Budget Audit Hours";
      public static readonly string CYBudgetAuditHoursResources = "CY Budget Audit Hours Resources";
      public static readonly string SpecialistHour = "Specialist Hour";
      public static readonly string SpecialistHourDetails = "Specialist Hour Details";
      public static readonly string CYForecastAuditHours = "CY Forecast Audit Hours";
      public static readonly string AuditHoursReconciliation = "Audit Hours Reconciliation";
      public static readonly string SubconCosts = "Subcon Costs";
      public static readonly string NonBillableExpenses = "Non Billable Expenses";
      public static readonly string GrossFees = "Gross Fees";
      public static readonly string BillingSchedule = "Billing Schedule";
      public static readonly string BillingScheduleDetail = "Billing Schedule Detail";
      public static readonly string BudgetGroupsComments = "Budget Groups Comments";
      public static readonly string BudgetGroupsHistories = "Budget Groups Histories";
      public static readonly string GrossFeeReason = "Gross Fee Reasons";
      public static readonly string BudgetAuditHour = "Budget Audit Hours";
      public static readonly string BudgetGroupLock = "Budget Group Locks";
      public static readonly string ChargeOutRate = "Charge Out Rates";
      public static readonly string CrsBooking = "Crs Bookings";
      public static readonly string CrsBookingRequest = "Crs Booking Requests";
      public static readonly string CrsBookingDailyFactor = "Crs Booking Daily Factors";
      public static readonly string CrsKeyDeadline = "Crs KeyDeadlines";
      public static readonly string CrsSubgroup = "Crs Subgroups";
      public static readonly string WipFees = "WipFees";
      public static readonly string TimesheetHour = "Timesheet Hours";
    }

    public static class AuditHoursReconciliationConstants
    {
      public static readonly string Audit = "Audit";

      public static readonly string AuditHoursReconciliationConstantsNotExisted =
        "Audit Hours Reconciliation Constants Not Existed !";
    }

    public static class StatusResponse
    {
      public static readonly string True = "True";
      public static readonly string Fail = "Fail";
    }

    public static class ColumnHeaderImportBudgetGroup
    {
      public static readonly string BudgetGroupIDHeader = "Budget Group ID";
      public static readonly string SubgroupIDHeader = "Subgroup ID*";
      public static readonly string PIEHeader = "PIE";
      public static readonly string NewClientHeader = "New Client";
      public static readonly string LastYearOfAuditHeader = "Last year of Audit";
      public static readonly string EQCRHeader = "EQCR";
      public static readonly string EPHeader = "EP*";
      public static readonly string CoEMHeader = "Co-EM";
      public static readonly string Remarks = "Remarks";
    }

    public static class ErrorMessageImport
    {
      public static readonly string MissingRequiredField = "Missing required field ";
      public static readonly string MissingRequiredFieldSubgroupID = "Subgroup ID";
      public static readonly string MissingRequiredFieldEp = "EP";
      public static readonly string MissingRequiredFieldEm = "Engagement Management ID";
      public static readonly string MissingRequiredFieldEpId = "Engagement Partner ID";
      public static readonly string MissingRequiredFieldBudgetGroupID = "Budget Group ID";
      public static readonly string MissingRequiredFieldEntityId = "Entity ID";
      public static readonly string MissingRequiredFieldTaxClassification = "Tax Classification";
      public static readonly string MissingRequiredFieldCcy = "Ccy";
      public static readonly string MissingRequiredFieldCurrentYearEngagementCode = "Current Year Engagement Code";
      public static readonly string MissingRequiredFieldClientNo = "ClientNo";
      public static readonly string MissingRequiredFieldAccountingStandard = "AccountingStandards";
      public static readonly string MissingRequiredFieldAuditingStandard = "AuditingStandards";
      public static readonly string InvalidBudgetGroupID = "Invalid Budget Group ID";
      public static readonly string BudgetGrouplocked = "Budget Group is locked";
      public static readonly string InvalidSubgroupID = "Invalid Subgroup ID";
      public static readonly string InvalidSubgroup = "Invalid Subgroup";
      public static readonly string InvalidEQCR = "Invalid EQCR";
      public static readonly string InvalidCoEM = "Invalid Co-EM";
      public static readonly string InvalidEP = "Invalid EP";
      public static readonly string Duplicate = "Duplicate";
      public static readonly string InvalidData = "Invalid data ";
      public static readonly string InvalidPyEngagementCode = "Invalid PY Engagement Code";
      public static readonly string InvalidCyEngagementCode = "Invalid CY Engagement Code";
      public static readonly string InvalidEntityID = "Invalid Entity ID";
      public static readonly string ValidatePie = "Primary entity PIE is Yes, unable to switch Budget Group PIE to No";
      public static readonly string BudgetExists = "Budget already exists";
      public static readonly string Valid = "Valid";
      public static readonly string Successfully = "Successfully";
      public static readonly string PrimaryTaggedEntityNull = "Primary Tagged Entity is null";
      public static readonly string WrongFileFormat = "Wrong file format. Please upload a valid file";
      public static readonly string TaxClassificationNotExist = "Tax Classification value must exist in the system";
      public static readonly string CcyNotExist = "CCY must exist in the system";
      public static readonly string EntityErrorTag = "The Entity has not been tagged to the current Budget Group";
      public static readonly string ValidReason = "Valid Reason for lost client is required";
      public static readonly string BudgetGroupIDDoNotMatch = "Budget Group ID does not match";
      public static readonly string BudgetGroupIsEditting = "Can not import this Budget group. Somebody is editing";
      public static readonly string BudgetGroupIsPendingApproval = "Budget Group is pending approval";
      public static readonly string ImportFail = "Import fail please check template file";
      public static readonly string MissingRequiredFieldCRSSubgroupId = "CRSSubgroupId";
      public static readonly string MissingRequiredFieldYearEnd = "YearEnd";
      public static readonly string BuDoesNotExist = "BU does not exist";
      public static readonly string InvalidEmployee = " invalid employee ID";
      public static readonly string ClientNoDoesNotExist = "Client No does not exist";
      public static readonly string InvalidAccountingStandards = "Invalid Accounting Standards";
      public static readonly string InvalidAuditingStandards = "Invalid Auditing Standards";
      public static readonly string InvalidExemptId = "Invalid ExemptId";
    }

    public static class ColumnHeaderImportTaggingEntities
    {
      public static readonly string BudgetGroupIDHeader = "Budget Group ID*";
      public static readonly string EntityIdHeader = "Entity ID*";
      public static readonly string EntityYearEndHeader = "Entity Year End";
      public static readonly string NewClientHeader = "New Client";
      public static readonly string LostClientHeader = "Lost Client";
      public static readonly string ReasonForLostClientHeader = "Reason for Lost Client";
      public static readonly string CurrentYearEngagementCodeHeader = "Current Year Engagement Code";
      public static readonly string PriorYearEngagementCodeHeader = "Prior Year Engagement Code";
      public static readonly string AccountingStandardsHeader = "Accounting Standards";
      public static readonly string AuditingStandardsHeader = "Auditing Standards";
    }

    public static class ColumnHeaderImportBillingSchedule
    {
      public static readonly string EntityIdHeader = "Entity ID*";
      public static readonly string BillingPartyHeader = "Billing party";
      public static readonly string TaxClassificationHeader = "Tax classification*";
      public static readonly string CcyHeader = "Ccy*";
      public static readonly string JanHeader = "Jan";
      public static readonly string FebHeader = "Feb";
      public static readonly string MarHeader = "Mar";
      public static readonly string AprHeader = "Apr";
      public static readonly string MayHeader = "May";
      public static readonly string JunHeader = "Jun";
      public static readonly string JulHeader = "Jul";
      public static readonly string AugHeader = "Aug";
      public static readonly string SepHeader = "Sep";
      public static readonly string OctHeader = "Oct";
      public static readonly string NovHeader = "Nov";
      public static readonly string DecHeader = "Dec";
    }

    public static class BudgetGroupAction
    {
      public static readonly string Approve = "Approve";
      public static readonly string Reject = "Reject";
    }

    public static class Number
    {
      public static readonly int Zero = 0;
      public static readonly int One = 1;
      public static readonly int Two = 2;
      public static readonly int Three = 3;
      public static readonly int Four = 4;
      public static readonly int Five = 5;
      public static readonly int Six = 6;
      public static readonly int Seven = 7;
      public static readonly int Eight = 8;
      public static readonly int Nine = 9;
      public static readonly int Ten = 10;
      public static readonly int Eleven = 11;
      public static readonly int Twelve = 12;
      public static readonly int Thirteen = 13;
      public static readonly int Fourteen = 14;
      public static readonly int Fifteen = 15;
      public static readonly int Sixteen = 16;
      public static readonly int Seventeen = 17;
      public static readonly int Eighteen = 18;
      public static readonly int Nineteen = 19;
      public static readonly int Twenty = 20;
      public static readonly int Mil = 1000000;
    }

    public static class EmailTemplate
    {
      public static readonly string RemindPendingApproval = "RemindPendingForApprovalBudgetGroup";
      public static readonly string RemindApprovedAndReject = "RemindApprovedAndReject";
      public static readonly string RequestDeleteBudgetGroups = "RequestDeleteBudgetGroups";
      public static readonly string SubmitBGtoAPICforEP = "SubmitBGtoAPICforEP";
      public static readonly string ArchivalDataForApic = "ArchivalDataForApic";
      public static readonly string RemindAPICNewBG = "RemindAPICNewBG";
      public static readonly string RemindUpdateBGForEP = "RemindUpdateBGForEP";
      public static readonly string RemindBudgetGroupIsLostClient = "RemindBudgetGroupIsLostClient";
      public static readonly string PendingRollForwardToManager = "PendingRollForwardToManager";
      public static readonly string PendingRollForwardToBuCoo = "PendingRollForwardToBuCoo";
      public static readonly string DraftRejectBudgetsAttachmentForBU = "DraftRejectBudgetsAttachmentForBU";
    }

    public static class PageUrl
    {
      public static readonly string MyPortfolio = "my-portfolio";
    }

    public static class TypeLock
    {
      public static readonly string Lock = "Lock";
      public static readonly string UnLock = "Unlock";
      public static readonly string Check = "Check";
      public static readonly string UnLockAll = "UnLockAll";
    }

    public static class ResourceGrade
    {
      public static readonly string P = "P";
      public static readonly string PA1 = "PA1";
      public static readonly string PA2 = "PA2";
      public static readonly string D1S = "D1S";
      public static readonly string D1 = "D1";
      public static readonly string D2 = "D2";
      public static readonly string M = "M";
    }

    public static class RollForwardMessage
    {
      public static readonly string BudgetGroupDoesNotSatisfyCondition =
        "Please select an Approved Budget Group that is recurring and is not a Lost Client to roll forward.";

      public static readonly string CanNotRollForwardRolling =
        "Can not roll forward this Budget group. Somebody is rolling";

      public static readonly string BudgetGroupAlreadyExists = "Budget Group already exists";
      public static readonly string FailToCreateSubgroup = "Fail to create Subgroup";
      public static readonly string NoteHistory = "Roll forward from Budget Group ID: ";

      public static readonly string CanNotRollForwardEditing =
        "Cannot roll forward this Budget Group because another user is editing it.";
    }

    public static class ColumnFilterWorkLoad
    {
      public static readonly string FY = "FY";
      public static readonly string Grade = "Grade";
      public static readonly string BusinessUnitName = "BU";
      public static readonly string EmployeeName = "Name";
      public static readonly string TargetChargeableHours = "targetChargeableHours";
      public static readonly string BudgetChargeableHours = "budgetchargeableHours";
      public static readonly string TargetBudgetVariance = "targetBudgetVariance";
      public static readonly string CYActualYTDHours = "cYActualYTDHours";
      public static readonly string TargetCYActualYTDVariance = "targetCYActualYTDVariance";
    }

    public static class StatusCheckPortfolio
    {
      public static readonly int IsSuccess = 0;
      public static readonly int TaggingSubgroup = 1;
      public static readonly int TaggingEntity = 2;
    }

    public static class ActionName
    {
      public static readonly string AddBudget = "Add BudgetGroup";
      public static readonly string UpdateBudget = "Update BudgetGroup";
      public static readonly string DeleteBudget = "Delete BudgetGroup";
      public static readonly string UpdateBudgetForAPIC = "Update Budget for APIC";
      public static readonly string UpdateTotalFinalisedFee = "Update Total Finalised Fee";
      public static readonly string EngagementHoursAudit = "Save Audit In Engagement Hours";
      public static readonly string ReportingFrameWork = "Save Reporting FrameWork";
      public static readonly string LockUnLockBudgetGroup = "Lock Or UnLock BudgetGroup";
      public static readonly string UnLockBudgetGroup = "UnLock BudgetGroup";
      public static readonly string LockBudgetGroup = "Lock BudgetGroup";
      public static readonly string ImportBudget = "Import Budget";
      public static readonly string AddTaggingEntity = "Add Tagging Entity";
      public static readonly string AddTaggingSubGroup = "Add Tagging SubGroup";
      public static readonly string DeleteCoEMs = "Delete CoEMs";
      public static readonly string AddCoEMs = "Add CoEMs";
      public static readonly string UpdateTaggingEntity = "Update TaggingEntity";
      public static readonly string DeleteTaggingEntity = "Delete TaggingEntity";
      public static readonly string UpdateTaggingSupGroup = "Update TaggingSupGroup";
      public static readonly string DeleteTaggingSubGroup = "Delete TaggingSubGroup";
      public static readonly string SaveGrosFee = "Save GrosFee";
      public static readonly string DeleteGrossFee = "Delete GrossFee";
      public static readonly string AddGrossFee = "Add GrossFee";
      public static readonly string UpdateCYForecast = "Update CY Forecast";
      public static readonly string UpdateAuditCYBudget = "Update Audit CYBudget";
      public static readonly string AddAuditCYBudget = "Add Audit CYBudget";
      public static readonly string AddSpecialistHour = "Add Specialist Hour";
      public static readonly string UpdateSpecialistHour = "Update Specialist Hour";
      public static readonly string DeleteSpecialistHour = "Delete Specialist Hour";
    }
  }
}