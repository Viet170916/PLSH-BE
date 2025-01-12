using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using BU.Services.Interface;
using Common.Enums;
using Data.Configurations;
using Data.Repository.Interfaces;
using Data.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Model.ResponseModel.Permission;

namespace BU.Services.Implementation
{
    public class CommonService : ICommonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppConfigSection _appSettings;
        private readonly IMemoryCache _cache;
        private readonly IHttpClientWrapper _httpClientWrapper;
        private readonly IMapper _mapper;

        public CommonService(IUnitOfWork unitOfWork, IOptions<AppConfigSection> options, IMemoryCache cache, IHttpClientWrapper httpClientWrapper, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _appSettings = options.Value;
            _cache = cache;
            _httpClientWrapper = httpClientWrapper;
            _mapper = mapper;
        }
        public async Task<UserPermissionResponse> GetUserInfo(HttpContext context)
        {
            // var username = GetNetworkId(context);
            // var user = _unitOfWork.UsersRepository.Find(u => u.NetworkId.ToLower() == username.ToLower() && u.IsActive).FirstOrDefault();
            // if (user == null)
            // {
            //     return null;
            // }
            // var userRoles = _unitOfWork.UserRolesRepository.FindQueryable(i => i.UserId == user.UserId && i.IsActive);
            // var roles = _unitOfWork.RolesRepository.FindQueryable(s => s.IsActive);
            // var userInfo = _unitOfWork.EmployeeRepository.Find(x => x.EmployeeId == user.EmployeeId).FirstOrDefault();
            // var roleIds = userRoles.Select(x => x.RoleId);
            // var roleIdInSession = GetCurrentUserRoleId(username);
            // UserRoles userRole = null;
            // if (roleIdInSession.HasValue && userRoles.FirstOrDefault(s => s.RoleId == roleIdInSession) != null)
            // {
            //     userRole = userRoles.FirstOrDefault(s => s.RoleId == roleIdInSession);
            // }
            // else
            // {
            //     userRole = userRoles.FirstOrDefault();
            // }
            // if (userRole == null)
            // {
            //     return null;
            // }
            // var role = roles.FirstOrDefault(x => x.RoleId == userRole.RoleId);
            // return new UserPermissionResponse()
            // {
            //     UserId = user.UserId,
            //     UserName = username,
            //     EmployeeName = userInfo.EmployeeName,
            //     UserRole = role?.RoleName,
            //     EmployeeId = user.EmployeeId,
            //     ImgUrl = userInfo.PhotoURL,
            //     BusinessUnit = userInfo.BusinessUnit,
            //     BusinessUnitName = userInfo.BusinessUnitName,
            //     UserRoles = roles.Where(z => roleIds.Contains(z.RoleId)).Select(x => new UserRoleResponse
            //     {
            //         RoleId = x.RoleId,
            //         RoleName = x.RoleName,
            //         RoleCode = x.RoleCode
            //     }).ToList(),
            // };
            return new UserPermissionResponse();
        }
        
        public string GetNetworkId(HttpContext context)
        {
            // var networkdId = context.User.Identity.Name;
            // Serilog.Log.Information($"DebugAccount {_appSettings.DebugAccount}");
            // if (!string.IsNullOrEmpty(_appSettings.DebugAccount))
            // {
            //     networkdId = _appSettings.DebugAccount;
            // }
            // var isEmail = Regex.IsMatch(networkdId, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
            //             RegexOptions.IgnoreCase);
            // if (isEmail)
            // {
            //     var commonUser = _unitOfWork.EmployeeRepository.FindQueryable(x => !string.IsNullOrEmpty(x.NetworkID) && x.EmailAddress.ToLower() == networkdId.ToLower()).FirstOrDefault();
            //     if (commonUser != null)
            //     {
            //         networkdId = commonUser.NetworkID;
            //     }
            // }
            // return networkdId;
            return null;
        }
        public long? GetCurrentUserRoleId(string networkId)
        {
            // var currentRoleId = _cache.Get<string>($"{Constants.SessionConstants.CurrentUserRole}-{networkId}");
            //
            // long? result = null;
            // if (!string.IsNullOrEmpty(currentRoleId))
            // {
            //     result = Convert.ToInt64(currentRoleId);
            // }
            // return result;
            return null;
        }
        // [ExcludeFromCodeCoverage]
    //     public async Task<IEnumerable<CommonEmployeeResponse>> GetNameBySubPartnerNameOrManager(CommonSearchRequest commonSearch)
    //     {
    //         if (!String.IsNullOrEmpty(commonSearch.PartnerName))
    //         {
    //             var names = from bg in _unitOfWork.BudgetGroupRepository.GetAllQueryable()
    //                         join ep in _unitOfWork.EmployeeRepository.GetAllQueryable() on bg.EP equals ep.EmployeeId
    //                         where ep.EmployeeName.Contains(commonSearch.PartnerName)
    //                             && (ep.Grade.Equals(GradeEnum.P.ToString()) || ep.Grade.Equals(GradeEnum.PA1.ToString()) || ep.Grade.Equals(GradeEnum.D1S.ToString()))
    //                         select ep;
    //             var result = await names.Select(n => new CommonEmployeeResponse()
    //             {
    //                 Id = n.EmployeeId,
    //                 NameEmployee = n.EmployeeName
    //             }).Distinct().ToListAsync().ConfigureAwait(false);
    //             return !result.Any() ? null : result;
    //         }
    //         else if (!String.IsNullOrEmpty(commonSearch.ManagerName))
    //         {
    //             var names = from bg in _unitOfWork.BudgetGroupRepository.GetAllQueryable()
    //                         join ep in _unitOfWork.EmployeeRepository.GetAllQueryable() on bg.EM equals ep.EmployeeId
    //                         where ep.EmployeeName.Contains(commonSearch.ManagerName)
    //                             && (ep.Grade.Equals(GradeEnum.AM.ToString())
    //                             || ep.Grade.Equals(GradeEnum.M.ToString())
    //                             || ep.Grade.Equals(GradeEnum.D1.ToString())
    //                             || ep.Grade.Equals(GradeEnum.D2.ToString())
    //                             || ep.Grade.Equals(GradeEnum.PA2.ToString()))
    //                         select ep;
    //             var result = await names.Select(n => new CommonEmployeeResponse()
    //             {
    //                 Id = n.EmployeeId,
    //                 NameEmployee = n.EmployeeName
    //             }).Distinct().ToListAsync().ConfigureAwait(false);
    //             return !result.Any() ? null : result;
    //         }
    //         else
    //         {
    //             return null;
    //         }
    //     }
    //     [ExcludeFromCodeCoverage]
    //     public async Task<IEnumerable<CommonBudgetGroupResponse>> GetBudgetGroupNameBySubName(string subName)
    //     {
    //         if (!String.IsNullOrEmpty(subName))
    //         {
    //             var result = await (from bg in _unitOfWork.BudgetGroupRepository.GetAllQueryable()
    //                                 where bg.BudgetGroupName.StartsWith(subName)
    //                                 select new CommonBudgetGroupResponse()
    //                                 {
    //                                     Id = bg.BudgetGroupId,
    //                                     BudgetName = bg.BudgetGroupName
    //                                 }).ToListAsync().ConfigureAwait(false);
    //
    //             return !result.Any() ? null : result;
    //         }
    //         else
    //         {
    //             return null;
    //         }
    //     }
    //     [ExcludeFromCodeCoverage]
    //     public async Task<IEnumerable<BuCommonResponse>> GetBUGroupNameBySubName(string buName, bool isFilter)
    //     {
    //         if (!String.IsNullOrEmpty(buName))
    //         {
    //             if (isFilter.Equals(true))
    //             {
    //                 var listBus = await (from b in _unitOfWork.BudgetGroupRepository.GetAllQueryable()
    //                                      join bu in _unitOfWork.BusinessUnitRepository.GetAllQueryable()
    //                                      on b.BU equals bu.No
    //                                      where !b.IsDeleted.Equals(true) && bu.Name.StartsWith(buName) && !bu.DeleteFlag.Equals(true)
    //                                      select new BuCommonResponse()
    //                                      {
    //                                          Bu = bu.Name,
    //                                          BuNo = bu.No
    //                                      }).Distinct().OrderByDescending(x => x.Bu).ToListAsync().ConfigureAwait(false);
    //                 return !listBus.Any() ? null : listBus;
    //             }
    //             else
    //             {
    //                 var listBus = await (from bu in _unitOfWork.BusinessUnitRepository.GetAllQueryable()
    //                                      where !bu.DeleteFlag.Equals(true) && bu.Name.StartsWith(buName)
    //                                      select new BuCommonResponse()
    //                                      {
    //                                          Bu = bu.Name,
    //                                          BuNo = bu.No
    //                                      }).Distinct().OrderByDescending(b => b.Bu).ToListAsync().ConfigureAwait(false);
    //                 return !listBus.Any() ? null : listBus;
    //             }
    //         }
    //         else
    //         {
    //             return null;
    //         }
    //     }
    //     [ExcludeFromCodeCoverage]
    //     public async Task<IEnumerable<CommonEmployeeResponse>> GetLastUpdatedBy(string search)
    //     {
    //         if (!String.IsNullOrEmpty(search))
    //         {
    //             var result = await (from bu in _unitOfWork.BudgetGroupRepository.FindQueryable(x => x.IsDeleted != true).Select(x => x.LastUpdatedBy).Distinct()
    //                                 join em in _unitOfWork.EmployeeRepository.GetAllQueryable()
    //                                 on bu equals em.EmployeeId into gf
    //                                 from em in gf.DefaultIfEmpty()
    //                                 where (bu.Contains(search) && Constants.SystemUser.Contains(search,StringComparison.OrdinalIgnoreCase)) || (em != null && em.EmployeeName.Contains(search))
    //                                 select new CommonEmployeeResponse()
    //                                 {
    //                                     Id = em.EmployeeId,
    //                                     NameEmployee = bu.Contains(search) && Constants.SystemUser.Contains(search, StringComparison.OrdinalIgnoreCase) ? Constants.SystemUser : em.EmployeeName
    //                                 }).ToListAsync().ConfigureAwait(false);
    //             return !result.Any() ? null : result;
    //         }
    //         else
    //         {
    //             return null;
    //         }
    //     }
    //     public async Task<IEnumerable<string>> GetKpmgFY(string kpmgFy)
    //     {
    //         if (!String.IsNullOrEmpty(kpmgFy))
    //         {
    //             var listPy = await _unitOfWork.BudgetGroupRepository
    //                 .FindQueryable(x => x.IsDeleted != true && x.FY != null)
    //                 .Select(x => x.FY.Value.ToString()).Distinct().ToListAsync().ConfigureAwait(false);
    //             var result = listPy.Where(x => x.StartsWith(kpmgFy));
    //             return !result.Any() ? null : result;
    //         }
    //         else
    //         {
    //             return null;
    //         }
    //     }
    //     public async Task<IEnumerable<TypeOfWork>> GetAllTypeOfWork()
    //     {
    //         return await _unitOfWork.TypeOfWorkRepository.GetAllQueryable().Select(z => new TypeOfWork
    //         {
    //             Id = z.Id,
    //             Name = z.Name,
    //             Code = z.Code,
    //         }).OrderBy(x => x.Name).ToListAsync().ConfigureAwait(false);
    //     }
    //     public async Task<List<Exempt>> GetAllExempt()
    //     {
    //         return await _unitOfWork.ExemptsRepository.FindQueryable(x => x.IsActive ?? true).ToListAsync().ConfigureAwait(false);
    //     }
    //     public async Task<List<Employee>> GetEmployeesBySearch(string search)
    //     {
    //         if (!String.IsNullOrWhiteSpace(search))
    //         {
    //             var result = await _unitOfWork.EmployeeRepository.FindQueryable(o => o.EmployeeName.Contains(search)
    //                       || o.EmployeeId == search).ToListAsync().ConfigureAwait(false);
    //             return !result.Any() ? null : result;
    //         }
    //         else
    //         {
    //             return null;
    //         }
    //     }
    //
    //     public async Task<List<BusinessUnit>> GetBusinessUnitBySearch(string search)
    //     {
    //         if (!String.IsNullOrWhiteSpace(search))
    //         {
    //             var result = await _unitOfWork.BusinessUnitRepository.FindQueryable(o => o.Name.StartsWith(search)
    //                       || o.No == search).ToListAsync().ConfigureAwait(false);
    //             return !result.Any() ? null : result;
    //         }
    //         else
    //         {
    //             return null;
    //         }
    //     }
    //
    //     public async Task<List<Client>> GetClientBySearch(string search)
    //     {
    //         if (!String.IsNullOrWhiteSpace(search))
    //         {
    //             var result = await _unitOfWork.ClientRepository.FindQueryable(o => o.Name.StartsWith(search)
    //                       || o.No == search).OrderBy(x => x.Name).ToListAsync().ConfigureAwait(false);
    //             return !result.Any() ? null : result;
    //         }
    //         else
    //         {
    //             return null;
    //         }
    //     }
    //
    //     public async Task<List<ClientGroup>> GetClientGroupBySearch(string search)
    //     {
    //         if (!String.IsNullOrWhiteSpace(search))
    //         {
    //             var result = await _unitOfWork.ClientGroupRepository.FindQueryable(o => o.Name.StartsWith(search)
    //                       || o.No == search).ToListAsync().ConfigureAwait(false);
    //             return !result.Any() ? null : result;
    //         }
    //         else
    //         {
    //             return null;
    //         }
    //     }
    //     public async Task<List<SubgroupInformationResponse>> GetSubgroupBySearch(SubgroupSearchRequest subgroupSearchRequest)
    //     {
    //         if (subgroupSearchRequest.BudgetGroupId.HasValue)
    //         {
    //             var objbuget = await _unitOfWork.BudgetGroupRepository
    //                 .FindQueryable(z => z.BudgetGroupId == subgroupSearchRequest.BudgetGroupId).FirstOrDefaultAsync()
    //                 .ConfigureAwait(false);
    //             if (objbuget != null)
    //             {
    //                 subgroupSearchRequest.YearEnd = objbuget.YearEnd?.ToString("dd/MM/yyyy");
    //             }
    //
    //             var listSubgroups1 = await _httpClientWrapper.GetAsync<List<SubgroupInformationResponse>>(_appSettings.DomainCrsApiUrl,
    //                              string.Format(Constants.UrlConst.GetSubgroupBySearch, subgroupSearchRequest.SubgroupName,
    //                              subgroupSearchRequest.Month, subgroupSearchRequest.Year, subgroupSearchRequest.TypeOfWork,
    //                              subgroupSearchRequest.ClientGroupId, subgroupSearchRequest.SubGroupId, subgroupSearchRequest.YearEnd))
    //                              .ConfigureAwait(false);
    //             var subgroupPrimary = await _unitOfWork.TaggingSubGroupRepository.FindQueryable(z => z.BudgetGroupId == subgroupSearchRequest.BudgetGroupId &&
    //                          z.IsPrimary == true && z.IsDeleted != true).Select(x => (long)(x.SubgroupId)).FirstOrDefaultAsync().ConfigureAwait(false);
    //             listSubgroups1 = listSubgroups1.Where(x => x.SubgroupId != subgroupPrimary).ToList();
    //             return !listSubgroups1.Any() ? null : listSubgroups1;
    //         }
    //         var listSubgroups = await _httpClientWrapper.GetAsync<List<SubgroupInformationResponse>>(_appSettings.DomainCrsApiUrl,
    //                              string.Format(Constants.UrlConst.GetSubgroupBySearch, subgroupSearchRequest.SubgroupName,
    //                              subgroupSearchRequest.Month, subgroupSearchRequest.Year, subgroupSearchRequest.TypeOfWork,
    //                              subgroupSearchRequest.ClientGroupId, subgroupSearchRequest.SubGroupId, subgroupSearchRequest.YearEnd))
    //                              .ConfigureAwait(false);
    //         if (subgroupSearchRequest.IsAddBudgetGroup == true)
    //         {
    //             var lstTaggingSubgroupPrimary = await _unitOfWork.TaggingSubGroupRepository.FindQueryable(z => listSubgroups.Select(x => x.SubgroupId)
    //                         .Contains(z.SubgroupId) && z.IsPrimary == true && z.IsDeleted != true).Select(x => (long)(x.SubgroupId)).ToListAsync().ConfigureAwait(false);
    //             listSubgroups = listSubgroups.Where(z => !lstTaggingSubgroupPrimary.Contains(z.SubgroupId)).ToList();
    //         }
    //
    //         return !listSubgroups.Any() ? null : listSubgroups;
    //     }
    //     public async Task<List<Employee>> GetEmployeesBySearchType(CommonSearchEmployee request, UserPermissionResponse currentEmployee)
    //     {
    //         if (!String.IsNullOrWhiteSpace(request.Search))
    //         {
    //             List<Employee> employees = null;
    //             if (Constants.SearchEmployeeType.COEMS.Equals(request.Type) || Constants.SearchEmployeeType.EM.Equals(request.Type))
    //             {
    //                 var listGradeCOEMS = new List<string>() { GradeEnum.PA2.ToString(), GradeEnum.D1.ToString(), GradeEnum.D2.ToString(),
    //                                                           GradeEnum.M.ToString(), GradeEnum.AM.ToString()};
    //                 employees = await _unitOfWork.EmployeeRepository.FindQueryable(o => o.EmployeeName.Contains(request.Search)
    //                              && !o.EmploymentStatus.Equals(Constants.StatusInactive) && listGradeCOEMS.Contains(o.Grade)
    //                             && o.Function.Equals(Constants.AuditFuntion) && (!request.IsBU || o.BusinessUnit == currentEmployee.BusinessUnit))
    //                             .OrderBy(x => x.EmployeeName)
    //                             .ToListAsync().ConfigureAwait(false);
    //             }
    //             else
    //             {
    //                 var listGradeEEQCR = new List<string>() { GradeEnum.P.ToString(), GradeEnum.PA1.ToString(), GradeEnum.D1S.ToString() };
    //                 employees = await _unitOfWork.EmployeeRepository.FindQueryable(o => o.EmployeeName.Contains(request.Search)
    //                             && !o.EmploymentStatus.Equals(Constants.StatusInactive) && listGradeEEQCR.Contains(o.Grade))
    //                             .OrderBy(x => x.EmployeeName)
    //                             .ToListAsync().ConfigureAwait(false);
    //             }
    //             return !employees.Any() ? null : employees;
    //         }
    //         else
    //         {
    //             return null;
    //         }
    //     }
    //     public async Task<List<EmployeeSearchResponse>> GetEmployeesByFilterSuggest(string search, string type)
    //     {
    //         if (!String.IsNullOrWhiteSpace(search))
    //         {
    //
    //             if (Constants.SearchEmployeeType.COEMS.Equals(type?.ToUpper()))
    //             {
    //                 var employees = await (from b in _unitOfWork.BudgetGroupRepository.GetAllQueryable()
    //                                        join bcr in _unitOfWork.BudgetGroupCOEMsRepository.GetAllQueryable()
    //                                        on b.BudgetGroupId equals bcr.BudgetGroupId
    //                                        join em in _unitOfWork.EmployeeRepository.GetAllQueryable()
    //                                        on bcr.EmployeeId equals em.EmployeeId
    //                                        where !b.IsDeleted.Equals(true) && !bcr.IsDelete.Equals(true)
    //                                        && em.EmployeeName.Contains(search)
    //                                        select new EmployeeSearchResponse()
    //                                        {
    //                                            EmployeeId = em.EmployeeId,
    //                                            EmployeeName = em.EmployeeName
    //                                        }).Distinct().OrderByDescending(e => e.EmployeeName).ToListAsync().ConfigureAwait(false);
    //                 return employees.Any() ? employees : null;
    //             }
    //             else if (Constants.SearchEmployeeType.EM.Equals(type?.ToUpper()))
    //             {
    //                 var listEMs = await (from b in _unitOfWork.BudgetGroupRepository.GetAllQueryable()
    //                                      join em in _unitOfWork.EmployeeRepository.GetAllQueryable()
    //                                      on b.EM equals em.EmployeeId
    //                                      where !b.IsDeleted.Equals(true) && em.EmployeeName.Contains(search)
    //                                      select new EmployeeSearchResponse()
    //                                      {
    //                                          EmployeeId = em.EmployeeId,
    //                                          EmployeeName = em.EmployeeName
    //                                      }).Distinct().OrderByDescending(x => x.EmployeeName).ToListAsync().ConfigureAwait(false);
    //                 return listEMs.Any() ? listEMs : null;
    //             }
    //             else if (Constants.SearchEmployeeType.EP.Equals(type?.ToUpper()))
    //             {
    //                 var listEPs = await (from b in _unitOfWork.BudgetGroupRepository.GetAllQueryable()
    //                                      join em in _unitOfWork.EmployeeRepository.GetAllQueryable()
    //                                      on b.EP equals em.EmployeeId
    //                                      where !b.IsDeleted.Equals(true) && em.EmployeeName.Contains(search)
    //                                      select new EmployeeSearchResponse()
    //                                      {
    //                                          EmployeeId = em.EmployeeId,
    //                                          EmployeeName = em.EmployeeName
    //                                      }).Distinct().OrderByDescending(x => x.EmployeeName).ToListAsync().ConfigureAwait(false);
    //                 return listEPs.Any() ? listEPs : null;
    //             }
    //             else if (Constants.SearchEmployeeType.EQCR.Equals(type?.ToUpper()))
    //             {
    //                 var listEQCRs = await (from b in _unitOfWork.BudgetGroupRepository.GetAllQueryable()
    //                                        join em in _unitOfWork.EmployeeRepository.GetAllQueryable()
    //                                        on b.EQCR equals em.EmployeeId
    //                                        where !b.IsDeleted.Equals(true) && em.EmployeeName.Contains(search)
    //                                        select new EmployeeSearchResponse()
    //                                        {
    //                                            EmployeeId = em.EmployeeId,
    //                                            EmployeeName = em.EmployeeName
    //                                        }).Distinct().OrderByDescending(x => x.EmployeeName).ToListAsync().ConfigureAwait(false);
    //                 return listEQCRs.Any() ? listEQCRs : null;
    //             }
    //             else
    //             {
    //                 return null;
    //             }
    //         }
    //         else
    //         {
    //             return null;
    //         }
    //     }
    //     public async Task<List<ClientGroupInformation>> GetClientById(string search)
    //     {
    //         var data = await (from qc in _unitOfWork.ClientRepository.GetAllQueryable()
    //                           join qcg in _unitOfWork.ClientGroupRepository.GetAllQueryable()
    //                           on qc.ClientGroupNo equals qcg.No
    //                           where qc.Name.StartsWith(search) && qc.DeleteFlag != true && qcg.DeleteFlag != true
    //                           orderby qc.Name
    //                           select new ClientGroupInformation()
    //                           {
    //                               ClientGroupId = qcg.Id,
    //                               ClientGroupName = qcg.Name,
    //                               ClientGroupNo = qcg.No,
    //                               ClientId = qc.Id,
    //                               ClientName = qc.Name,
    //                               ClientNo = qc.No,
    //                               YearEndMonth = qc.YearEndMonth
    //                           }
    //                     ).ToListAsync().ConfigureAwait(false);
    //         return data;
    //     }
    //     public async Task<List<ReasonsForLostClientResponse>> GetReasonsForLostClient()
    //     {
    //         var result = await _unitOfWork.ReasonsForLostClientRepository.GetAllQueryable().Select(res => new ReasonsForLostClientResponse()
    //         {
    //             ReasonsForLostClientId = res.ReasonsForLostClientId,
    //             ReasonsForLostClientCode = res.ReasonsForLostClientCode,
    //             ReasonsForLostClientName = res.ReasonsForLostClientName
    //         }).OrderBy(re => re.ReasonsForLostClientName).ToListAsync().ConfigureAwait(false);
    //         return result;
    //     }
    //
    //     public async Task<List<ClientForTaggingEntityResponse>> GetClientNotExistTaggingEntity(int budgetGroupId)
    //     {
    //         try
    //         {
    //             var clients = await (from b in _unitOfWork.BudgetGroupRepository.GetAllQueryable()
    //                                  join c in _unitOfWork.ClientRepository.GetAllQueryable()
    //                                  on b.ClientGroupId equals c.ClientGroupNo
    //                                  join te in _unitOfWork.TaggingEntityRepository.GetAllQueryable()
    //                                  on new { c.No, b.BudgetGroupId } equals new { No = te.ClientNo, BudgetGroupId = te.BudgetGroupId.Value } into gte
    //                                  from te in gte.DefaultIfEmpty()
    //                                  join ex in _unitOfWork.ExemptsRepository.GetAllQueryable() on te.ExemptId equals ex.ExemptId into e
    //                                  from ex in e.DefaultIfEmpty()
    //                                  join r in _unitOfWork.ReasonsForLostClientRepository.GetAllQueryable()
    //                                     on te.ReasonsForLostClient equals r.ReasonsForLostClientId into rs
    //                                  from r in rs.DefaultIfEmpty()
    //                                  where !c.DeleteFlag.Equals(true) && b.BudgetGroupId.Equals(budgetGroupId)
    //                                  && !te.IsDeleted.Equals(true)
    //                                  select new ClientForTaggingEntityResponse()
    //                                  {
    //                                      ClientGroupName = b.ClientGroupName,
    //                                      ClientName = c.Name,
    //                                      EntityId = c.Id,
    //                                      ClientNo = c.No,
    //                                      YearEndMonth = b.YearEnd,
    //                                      LostClient = te.LostClient,
    //                                      NewClient = te.NewClient,
    //                                      CYEngtCodes = te.CYEngtCodes,
    //                                      PYEngtCodes = te.PYEngtCodes,
    //                                      TaggingEntityYearEnd = te.YearEnd,
    //                                      Exempt = new Model.ResponseModel.Exempts.Exempt()
    //                                      {
    //                                          ExemptId = ex.ExemptId,
    //                                          ExemptCode = ex.ExemptCode,
    //                                          ExemptName = ex.ExemptName
    //                                      },
    //                                      ReasonsForLostClient = new ReasonsForLostClientResponse()
    //                                      {
    //                                          ReasonsForLostClientId = r.ReasonsForLostClientId,
    //                                          ReasonsForLostClientCode = r.ReasonsForLostClientCode,
    //                                          ReasonsForLostClientName = r.ReasonsForLostClientName
    //                                      },
    //                                      IsSelected = (te != null && te.IsDeleted != true)
    //                                  }).Distinct().OrderByDescending(c => c.ClientName).ToListAsync().ConfigureAwait(false);
    //             return clients;
    //         }
    //         catch (Exception ex)
    //         {
    //             throw new Exception(ex.Message);
    //         }
    //     }
    //
    //     public async Task<List<EngagementDetailResponse>> GetEngagementByClientNo(string clientNo)
    //     {
    //         var engagement = await _unitOfWork.EngagementDetailRepository.GetAllQueryable().Where(eg => eg.ClientNo.Equals(clientNo))
    //             .Select(eg => new EngagementDetailResponse()
    //             {
    //                 EngagementDetailId = eg.EngagementDetailId,
    //                 EngagementName = eg.EngagementName,
    //                 EngagementCode = eg.EngagementNo
    //             })
    //             .ToListAsync().ConfigureAwait(false);
    //         return engagement;
    //     }
    //     public async Task<List<SpecialistType>> GetAllSpecialList()
    //     {
    //         return await _unitOfWork.SpecialistTypeRepository.FindQueryable(x => x.IsActive ?? true).ToListAsync().ConfigureAwait(false);
    //     }
    //
    //     public async Task<List<ClientForTaggingEntityResponse>> GetClientByClientGroupOfBudgetGroup(int budgetGroupId)
    //     {
    //         try
    //         {
    //             var budgetGroup = await _unitOfWork.BudgetGroupRepository.GetById(budgetGroupId).ConfigureAwait(false);
    //             var clients = await (from t in _unitOfWork.TaggingEntityRepository.GetAllQueryable()
    //                                  join c in _unitOfWork.ClientRepository.GetAllQueryable() on t.ClientNo equals c.No into tc
    //                                  from c in tc.DefaultIfEmpty()
    //                                  where !t.IsPrimary.Equals(true) && c.ClientGroupNo.Equals(budgetGroup.ClientGroupId)
    //                                  select new ClientForTaggingEntityResponse()
    //                                  {
    //                                      ClientName = c.Name,
    //                                      EntityId = c.Id,
    //                                      ClientNo = c.No,
    //                                      YearEndMonth = c.YearEndMonth,
    //                                      TaggingEntityYearEnd = t.YearEnd,
    //                                      CYEngtCodes = t.CYEngtCodes,
    //                                      PYEngtCodes = t.PYEngtCodes,
    //                                      LostClient = t.LostClient,
    //                                      NewClient = t.NewClient
    //                                  }).ToListAsync().ConfigureAwait(false);
    //             return clients;
    //         }
    //         catch (Exception e)
    //         {
    //             throw new Exception(e.Message);
    //         }
    //     }
    //
    //     public async Task<List<SubgroupSimpleInformationResponse>> GetSubgroupByClientId(long clientId)
    //     {
    //         var clientNo = _unitOfWork.ClientRepository.FindQueryable(x => x.Id.Equals(clientId)).FirstOrDefault().No;
    //         var listSubgroupInfo = await _httpClientWrapper
    //              .GetAsync<List<SubgroupSimpleInformationResponse>>(_appSettings.DomainCrsApiUrl, Constants.UrlConst.GetSubgroupInfo + clientNo).ConfigureAwait(false);
    //
    //         var lstTaggingSubgroupPrimary = await _unitOfWork.TaggingSubGroupRepository.FindQueryable(z => listSubgroupInfo.Select(x => x.SubgroupId)
    //                       .Contains(z.SubgroupId) && z.IsPrimary == true && z.IsDeleted != true).Select(x => (long)(x.SubgroupId)).ToListAsync().ConfigureAwait(false);
    //         listSubgroupInfo = listSubgroupInfo.Where(z => !lstTaggingSubgroupPrimary.Contains(z.SubgroupId)).ToList();
    //         return listSubgroupInfo;
    //     }
    //     public async Task<List<ProfitCenterInformation>> GetProfitCentersByBu(string buNo)
    //     {
    //         var listProfitCenter = await _httpClientWrapper
    //            .GetAsync<List<ProfitCenterInformation>>(_appSettings.DomainCrsApiUrl, Constants.UrlConst.GetProfitCenterByBu + buNo).ConfigureAwait(false);
    //         return listProfitCenter;
    //
    //     }
    //
    //     public async Task<List<AccountingStandardResponse>> GetAccountingStandardList()
    //     {
    //         var accountingStandards = await _unitOfWork.AccountingStandardRepository.FindQueryable(x => x.IsActive).OrderBy(o => o.AccountingStandardname).ToListAsync().ConfigureAwait(false);
    //         return _mapper.Map<List<AccountingStandardResponse>>(accountingStandards);
    //     }
    //
    //     public async Task<List<AuditingStandardResponse>> GetAuditingStandardList()
    //     {
    //         var audittingStandards = await _unitOfWork.AuditingStandardRepository.FindQueryable(x => x.IsActive).OrderBy(o => o.AuditingStandardName).ToListAsync().ConfigureAwait(false);
    //         return _mapper.Map<List<AuditingStandardResponse>>(audittingStandards);
    //     }
    //
    //     public async Task<List<CountryResponse>> GetCountryList(string search)
    //     {
    //         if (!String.IsNullOrWhiteSpace(search))
    //         {
    //             var country = await _unitOfWork.CountryRepository.FindQueryable(x => x.CountryName.ToLower().StartsWith(search.ToLower()))
    //                         .OrderBy(x => x.CountryName).ToListAsync().ConfigureAwait(false);
    //             return _mapper.Map<List<CountryResponse>>(country);
    //         }
    //         else
    //         {
    //             return null;
    //         }
    //
    //     }
    //
    //     public async Task<IEnumerable<CurrencyCommon>> GetCurrencyList(string search)
    //     {
    //         var now = DateTime.Today;
    //         var list = await _unitOfWork.ExchangeRateRepository
    //             .FindQueryable(x => string.IsNullOrEmpty(search)
    //             || (x.FromCurrency.StartsWith(search) && x.ToCurrency == Constants.ToCurrency)).ToListAsync()
    //             .ConfigureAwait(false);
    //         return GroupCurrencyByList(list);
    //     }
    //
    //     public async Task<List<GrossFeeReasonTypeResponse>> GetGrossFeeReasonTypeList()
    //     {
    //         var grossFeeReasonType = await _unitOfWork.GrossFeeReasonTypeRepository.FindQueryable(x => x.IsActive).OrderBy(o => o.GrossFeeReasonName).ToListAsync().ConfigureAwait(false);
    //         return _mapper.Map<List<GrossFeeReasonTypeResponse>>(grossFeeReasonType);
    //     }
    //     public IEnumerable<CurrencyCommon> GroupCurrencyByList(IEnumerable<ExchangeRates> data)
    //     {
    //         var now = DateTime.Today;
    //         var result = data.GroupBy(person => person.FromCurrency)
    //                             .OrderBy(group => group.Key)
    //                             .Select(group => group.OrderByDescending(currency => currency.ValidTo >= now ? currency.ValidFrom : currency.ValidTo))
    //                             .Select((currency) => new CurrencyCommon()
    //                             {
    //                                 Currency = currency.FirstOrDefault()?.FromCurrency,
    //                                 ToSGDRate = currency.FirstOrDefault(x => (x.ValidFrom <= now && x.ValidTo >= now) || x.ValidTo <= now)?.ToSGDRate ?? 0
    //                             })
    //                             .ToList();            
    //         return result;
    //     }
    //     public async Task<List<ClientSimpleInformation>> GetClientFromTagging(int budgetGroupId)
    //     {
    //         var data = await (from t in _unitOfWork.TaggingEntityRepository.GetAllQueryable()
    //                           join c in _unitOfWork.ClientRepository.GetAllQueryable() on
    //                           t.ClientNo equals c.No
    //                           where t.BudgetGroupId == budgetGroupId && t.IsDeleted != true
    //                           select new ClientSimpleInformation()
    //                           {
    //                               ClientId = c.Id,
    //                               ClientNo = c.No,
    //                               ClientName = c.Name
    //                           }).Distinct().OrderBy(o => o.ClientName).ToListAsync().ConfigureAwait(false);
    //         return data;
    //     }
    //     public async Task<IEnumerable<TaxClassification>> GetTaxClassification()
    //     {
    //         return await _unitOfWork.TaxClassificationRepository.FindQueryable(x => x.IsActive == true).ToListAsync().ConfigureAwait(false);
    //     }
    //
    //
    //     public bool CheckPercentTotalCYBudge(decimal newlyCYbudget, decimal oldCYbudget)
    //     {
    //         var result = oldCYbudget != 0 ? (newlyCYbudget - oldCYbudget) * Constants.OneHundred / oldCYbudget : 0;
    //         if (Math.Abs(result) >= Constants.Number.Five)
    //         {
    //             return true;
    //         }
    //         return false;
    //     }
    //     public bool CheckBudgetApproved(int budgetGroupId)
    //     {
    //         var result = _unitOfWork.BudgetGroupRepository.FindQueryable(x => x.BudgetGroupId == budgetGroupId && x.IsDeleted != true && x.IsLocked == false
    //         && x.BudgetGroupStatus.Value == (int)BudgetGroupStatusEnum.Approved).FirstOrDefault();
    //         if (result != null)
    //         {
    //             return true;
    //         }
    //         return false;
    //     }
    //     public async Task UpdateStatusBudgetGroup(int budgetGroupId, string employeeId)
    //     {
    //         var budget = await _unitOfWork.BudgetGroupRepository.GetById(budgetGroupId).ConfigureAwait(false);
    //         budget.BudgetGroupStatus = (int)BudgetGroupStatusEnum.Pending_Approval;
    //         budget.AuditUpdated(employeeId);
    //         await _unitOfWork.BudgetGroupRepository.Update(budget, employeeId).ConfigureAwait(false);
    //     }
    //     public async Task<BookingRequestDailyFactor> GetBookingRequestDaily(int budgetGroupId)
    //     {
    //         var subGroupId = _unitOfWork.TaggingSubGroupRepository.FindQueryable(x => x.BudgetGroupId == budgetGroupId)
    //                         .Where(x => x.IsDeleted != true).Select(x => x.SubgroupId).ToList();
    //
    //         return await _httpClientWrapper.PostMultiAsync<BookingRequestDailyFactor>(_appSettings.DomainCrsApiUrl,
    //                                 Constants.UrlConst.GetBookingRequestDailyFactor, subGroupId).ConfigureAwait(false);
    //     }
    //
    //     public async Task<List<UserPermissionResponse>> GetUsersByApicRole()
    //     {
    //         var role = await _unitOfWork.RolesRepository.FindQueryable(x => x.RoleCode == "APIC").FirstOrDefaultAsync().ConfigureAwait(false);
    //         var userRoleApic = await _unitOfWork.UserRolesRepository.FindQueryable(x => x.RoleId == role.RoleId && x.IsActive).Select(s => s.UserId).ToListAsync().ConfigureAwait(false);
    //         var employeeRoleApic = await _unitOfWork.UsersRepository.FindQueryable(x => userRoleApic.Contains(x.UserId) && x.IsActive).Select(s => s.EmployeeId).ToListAsync().ConfigureAwait(false);
    //
    //         var employeeInfo = await _unitOfWork.EmployeeRepository.FindQueryable(x => employeeRoleApic.Contains(x.EmployeeId)).ToListAsync().ConfigureAwait(false);
    //
    //         return employeeInfo.Select(s => new UserPermissionResponse
    //         {
    //             EmployeeId = s.EmployeeId,
    //             EmployeeName = s.EmployeeName,
    //             EmailAddress = s.EmailAddress
    //         }).ToList();
    //     }
    //
    //     public string GetValueCell(ICell cell)
    //     {
    //         var result = "";
    //         try
    //         {
    //             if (cell != null)
    //             {
    //                 result = cell.ToString();
    //             }
    //         }
    //         catch (Exception ex)
    //         {
    //             throw;
    //         }
    //         return result;
    //     }
    //
    //     public ISheet GetDataFromExcel(IFormFile file)
    //     {
    //         HSSFWorkbook hssfwb;
    //         XSSFWorkbook xssfwb;
    //         ISheet sheet = null;
    //         if (Path.GetExtension(file?.FileName).ToLower() == ".xls")
    //         {
    //             hssfwb = new HSSFWorkbook(file?.OpenReadStream());
    //             sheet = hssfwb.GetSheetAt(0);
    //         }
    //         else if (Path.GetExtension(file?.FileName).ToLower() == ".xlsx")
    //         {
    //             xssfwb = new XSSFWorkbook(file?.OpenReadStream());
    //             sheet = xssfwb.GetSheetAt(0);
    //         }
    //         else
    //         {
    //             return sheet;
    //         }
    //         return sheet;
    //     }
    //     public bool? IsEMOrCOem(UserPermissionResponse currentEmployee)
    //     {
    //         return currentEmployee?.UserRoles.Any(x => x.RoleCode.Equals(RoleEnum.COEM.ToString())
    //               || x.RoleCode.Equals(RoleEnum.EM.ToString()));
    //     }
    //     public bool? IsEMOrCOemOrApic(UserPermissionResponse currentEmployee)
    //     {
    //         return currentEmployee?.UserRoles.Any(x => x.RoleCode.Equals(RoleEnum.COEM.ToString())
    //               || x.RoleCode.Equals(RoleEnum.EM.ToString()) || x.RoleCode.Equals(RoleEnum.APIC.ToString()));
    //     }
    //     public bool? IsEMCOemOrEP(UserPermissionResponse currentEmployee)
    //     {
    //         return currentEmployee?.UserRoles.Any(x => x.RoleCode.Equals(RoleEnum.COEM.ToString()) || x.RoleCode.Equals(RoleEnum.EP.ToString())
    //               || x.RoleCode.Equals(RoleEnum.EM.ToString()));
    //     }
    //     public bool? IsAPIC(UserPermissionResponse currentEmployee)
    //     {
    //         return currentEmployee?.UserRoles.Any(x => x.RoleCode.Equals(RoleEnum.APIC.ToString()));
    //     }
    //     public async Task<List<BusinessUnitResponse>> GetAllBu()
    //     {
    //         return await _unitOfWork.BusinessUnitRepository.FindQueryable(x => x.DeleteFlag != true).Select(o => new BusinessUnitResponse()
    //         {
    //             BuNo = o.No,
    //             BuName = o.Name,
    //             Function = o.Function
    //         }).OrderBy(m => m.BuName).ToListAsync().ConfigureAwait(false);
    //     }
    //     public async Task<bool?> UpdateBudgetGroupPrimary(BudgetGroup budgetGroup, bool? isPrimary, UserPermissionResponse currentEmployee)
    //     {
    //         if (isPrimary.Equals(true) && budgetGroup is not null && budgetGroup?.BudgetGroupStatus == (int)BudgetGroupStatusEnum.Approved)
    //         {
    //             budgetGroup.BudgetGroupStatus = (int)BudgetGroupStatusEnum.Pending_Approval;
    //             budgetGroup.StatusAPIC = null;
    //             budgetGroup.AuditUpdated(currentEmployee.EmployeeId);
    //             await _unitOfWork.BudgetGroupRepository.Update(budgetGroup, currentEmployee.EmployeeId).ConfigureAwait(false);
    //         }
    //         return true;
    //     }
    //     public async Task<List<int?>> GetListFY()
    //     {
    //         return await _unitOfWork.BudgetGroupRepository.FindQueryable(x => x.IsDeleted != true && x.FY != null)
    //             .Select(x => x.FY).Distinct().ToListAsync().ConfigureAwait(false);
    //     }
    //     public IEnumerable<GradeResponse> GetListGrade()
    //     {
    //         var result = new List<GradeResponse>()
    //         {
    //             new GradeResponse()
    //             {
    //                 Code="P",
    //                 IsManagement = true,
    //                 Name = "P"
    //             },
    //             new GradeResponse()
    //             {
    //                 Code="PA1",
    //                 IsManagement = true,
    //                 Name = "PA1"
    //             },
    //             new GradeResponse()
    //             {
    //                 Code="PA2",
    //                 IsManagement = true,
    //                 Name = "PA2"
    //             },
    //             new GradeResponse()
    //             {
    //                 Code="D1S",
    //                 IsManagement = true,
    //                 Name = "D1S"
    //             },
    //             new GradeResponse()
    //             {
    //                 Code="D1",
    //                 IsManagement = true,
    //                 Name = "D1"
    //             },
    //             new GradeResponse()
    //             {
    //                 Code="D2",
    //                 IsManagement = true,
    //                 Name = "D2"
    //             },
    //             new GradeResponse()
    //             {
    //                 Code="M",
    //                 IsManagement = true,
    //                 Name = "M"
    //             }
    //
    //         };
    //         return result;
    //     }
    //     public List<string> GetTPCCostCenterCodeBySettingKey()
    //     {
    //
    //         var setting = _unitOfWork.SettingRepository.FindQueryable(z => z.SettingCode == Common.Library.Constants.SettingKeyConst.TAX_Function_Default_CC_For_Costing).FirstOrDefault();
    //         var lstCodeSettings = new List<string>();
    //         if (setting != null && !string.IsNullOrEmpty(setting.SettingValue))
    //         {
    //             lstCodeSettings = setting.SettingValue.Split(';').Where(z => !string.IsNullOrEmpty(z)).ToList();
    //         }
    //         if (lstCodeSettings.Count > 0)
    //         {
    //             return _unitOfWork.CostCenterRepository.FindQueryable(x => x.ActiveFlag != 0 && lstCodeSettings.Contains(x.CostCenterCode))
    //                            .Select(o => o.CostCenterCode).ToList();
    //         }
    //         return new List<string>();
    //     }
    }
}
