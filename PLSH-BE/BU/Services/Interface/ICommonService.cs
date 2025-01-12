namespace BU.Services.Interface
{
    public interface ICommonService
    {
        // Task<UserPermissionResponse> GetUserInfo(HttpContext context);
        // string GetNetworkId(HttpContext context);
        // long? GetCurrentUserRoleId(string networkId);
        // Task<IEnumerable<CommonBudgetGroupResponse>> GetBudgetGroupNameBySubName(string subName);
        // Task<IEnumerable<BuCommonResponse>> GetBUGroupNameBySubName(string buName, bool isFilter);
        // Task<IEnumerable<CommonEmployeeResponse>> GetNameBySubPartnerNameOrManager(CommonSearchRequest commonSearch);
        // Task<IEnumerable<TypeOfWork>> GetAllTypeOfWork();
        // Task<List<Exempt>> GetAllExempt();
        // Task<List<Employee>> GetEmployeesBySearch(string search);
        // Task<List<BusinessUnit>> GetBusinessUnitBySearch(string search);
        // Task<List<Client>> GetClientBySearch(string search);
        // Task<List<ClientGroup>> GetClientGroupBySearch(string search);
        // Task<List<SubgroupInformationResponse>> GetSubgroupBySearch(SubgroupSearchRequest subgroupSearchRequest);
        // Task<List<Employee>> GetEmployeesBySearchType(CommonSearchEmployee request,UserPermissionResponse currentEmployee);
        // Task<List<EmployeeSearchResponse>> GetEmployeesByFilterSuggest(string search, string type);
        // Task<List<ClientGroupInformation>> GetClientById(string search);
        // Task<List<ReasonsForLostClientResponse>> GetReasonsForLostClient();
        // Task<List<ClientForTaggingEntityResponse>> GetClientNotExistTaggingEntity(int budgetGroupId);
        // Task<List<EngagementDetailResponse>> GetEngagementByClientNo(string clientNo);
        // Task<List<SpecialistType>> GetAllSpecialList();
        // Task<List<ClientForTaggingEntityResponse>> GetClientByClientGroupOfBudgetGroup(int budgetGroupId);
        // Task<List<SubgroupSimpleInformationResponse>> GetSubgroupByClientId(long clientId);
        // Task<List<ProfitCenterInformation>> GetProfitCentersByBu(string buNo);
        // Task<List<AccountingStandardResponse>> GetAccountingStandardList();
        // Task<List<AuditingStandardResponse>> GetAuditingStandardList();
        // Task<List<CountryResponse>> GetCountryList(string search);
        // Task<IEnumerable<CurrencyCommon>> GetCurrencyList(string search);
        // Task<List<GrossFeeReasonTypeResponse>> GetGrossFeeReasonTypeList();
        // IEnumerable<CurrencyCommon> GroupCurrencyByList(IEnumerable<ExchangeRates> data);
        // Task<List<ClientSimpleInformation>> GetClientFromTagging(int budgetGroupId);
        // Task<IEnumerable<string>> GetKpmgFY(string kpmgFy);
        // Task<IEnumerable<TaxClassification>> GetTaxClassification();
        // Task<BookingRequestDailyFactor> GetBookingRequestDaily(int budgetGroupId);
        // bool CheckPercentTotalCYBudge(decimal newlyCYbudget, decimal oldCYbudget);
        // Task UpdateStatusBudgetGroup(int budgetGroupId, string employeeId);
        // Task<IEnumerable<CommonEmployeeResponse>> GetLastUpdatedBy(string search);
        // Task<List<UserPermissionResponse>> GetUsersByApicRole();
        // bool CheckBudgetApproved(int budgetGroupId);
        // string GetValueCell(ICell cell);
        // ISheet GetDataFromExcel(IFormFile file);
        // bool? IsEMOrCOem(UserPermissionResponse currentEmployee);
        // bool? IsEMCOemOrEP(UserPermissionResponse currentEmployee);
        // bool? IsAPIC(UserPermissionResponse currentEmployee);
        // bool? IsEMOrCOemOrApic(UserPermissionResponse currentEmployee);
        // Task<List<BusinessUnitResponse>> GetAllBu();
        // Task<bool?> UpdateBudgetGroupPrimary(BudgetGroup budgetGroup, bool? isPrimary, UserPermissionResponse currentEmployee);
        // Task<List<int?>> GetListFY();
        // IEnumerable<GradeResponse> GetListGrade();
        // List<string> GetTPCCostCenterCodeBySettingKey();
    }
}
