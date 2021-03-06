﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using AutoMapper;
using Microsoft.AspNet.Identity.Owin;
using TEDU.Common.Constants;
using TEDU.Common.Exceptions;
using TEDU.Model.Models;
using TEDU.Service;
using TEDU.Web.App_Start;
using TEDU.Web.Infrastructure.Core;
using TEDU.Web.Infrastructure.Extensions;
using TEDU.Web.ViewModels;

namespace TEDU.Web.Api
{
    [RoutePrefix("api/account")]
    [Authorize]
    public class AccountController : ApiControllerBase
    {
        private readonly IAppGroupService _appGroupService;
        private readonly IAppRoleService _appRoleService;
        private readonly ApplicationSignInManager _signInManager;
        private readonly ApplicationUserManager _userManager;

        public AccountController(
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            IAppGroupService appGroupService,
            IAppRoleService appRoleService,
            IErrorService errorService)
            : base(errorService)
        {
            _userManager = userManager;
            _appGroupService = appGroupService;
            _signInManager = signInManager;
            _appRoleService = appRoleService;
        }

        [Route("getlistpaging")]
        [HttpGet]
        public HttpResponseMessage GetListPaging(HttpRequestMessage request, int page, int pageSize,
            string filter = null)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                var totalRow = 0;
                var model = _userManager.Users;
                var modelVm = Mapper.Map<IEnumerable<AppUser>, IEnumerable<AppUserViewModel>>(model);

                var pagedSet = new PaginationSet<AppUserViewModel>
                {
                    Page = page,
                    TotalCount = totalRow,
                    TotalPages = (int) Math.Ceiling((decimal) totalRow/pageSize),
                    Items = modelVm
                };

                response = request.CreateResponse(HttpStatusCode.OK, pagedSet);

                return response;
            });
        }

        [Route("getlisttrainer")]
        [HttpGet]
        public HttpResponseMessage GetListTrainer(HttpRequestMessage request)
        {
            return CreateHttpResponse(request, () =>
            {
                HttpResponseMessage response = null;
                var model = _appGroupService.GetListUserByGroupName(CommonConstants.TRAINER_GROUP_NAME);

                var modelVm = Mapper.Map<IEnumerable<AppUser>, IEnumerable<AppUserViewModel>>(model);

                response = request.CreateResponse(HttpStatusCode.OK, modelVm);

                return response;
            });
        }

        [Route("detail/{id}")]
        [HttpGet]
        public HttpResponseMessage Details(HttpRequestMessage request, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return request.CreateErrorResponse(HttpStatusCode.BadRequest, nameof(id) + " không có giá trị.");
            }
            var user = _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return request.CreateErrorResponse(HttpStatusCode.NoContent, "Không có dữ liệu");
            }
            var appUserViewModel = Mapper.Map<AppUser, AppUserViewModel>(user.Result);
            var groups = _appGroupService.GetListGroupByUserId(appUserViewModel.Id);

            appUserViewModel.AppGroups = Mapper.Map<IEnumerable<AppGroup>, IEnumerable<AppGroupViewModel>>(groups);
            return request.CreateResponse(HttpStatusCode.OK, appUserViewModel);
        }

        [HttpPost]
        [Route("add")]
        [Authorize(Roles = "AddUser")]
        public async Task<HttpResponseMessage> Create(HttpRequestMessage request, AppUserViewModel appUserViewModel)
        {
            if (ModelState.IsValid)
            {
                var newAppUser = new AppUser();
                newAppUser.UpdateUser(appUserViewModel);
                try
                {
                    newAppUser.Id = Guid.NewGuid().ToString();
                    var result = await _userManager.CreateAsync(newAppUser, appUserViewModel.Password);
                    if (result.Succeeded)
                    {
                        //add account to group
                        var userGroups = new List<AppUserGroup>();
                        foreach (var group in appUserViewModel.AppGroups)
                        {
                            userGroups.Add(new AppUserGroup
                            {
                                UserId = newAppUser.Id,
                                GroupId = group.Id
                            });

                            //add role to user
                            var listRole = _appRoleService.GetListRoleByGroupId(group.Id);
                            foreach (var role in listRole)
                            {
                                await _userManager.RemoveFromRoleAsync(newAppUser.Id, role.Name);
                                await _userManager.AddToRoleAsync(newAppUser.Id, role.Name);
                            }
                        }
                        _appGroupService.AddUserToGroups(userGroups, newAppUser.Id);
                        _appGroupService.Save();

                        return request.CreateResponse(HttpStatusCode.OK, appUserViewModel);
                    }
                    return request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Join(",", result.Errors));
                }
                catch (NameDuplicatedException dex)
                {
                    return request.CreateErrorResponse(HttpStatusCode.BadRequest, dex.Message);
                }
                catch (Exception ex)
                {
                    return request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
            return request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        [HttpPut]
        [Route("update")]
        [Authorize(Roles = "EditUser")]
        public async Task<HttpResponseMessage> Update(HttpRequestMessage request, AppUserViewModel appUserViewModel)
        {
            if (ModelState.IsValid)
            {
                var appUser = await _userManager.FindByIdAsync(appUserViewModel.Id);
                try
                {
                    appUser.UpdateUser(appUserViewModel);
                    var result = await _userManager.UpdateAsync(appUser);
                    if (result.Succeeded)
                    {
                        var userGroups = new List<AppUserGroup>();
                        foreach (var group in appUserViewModel.AppGroups)
                        {
                            userGroups.Add(new AppUserGroup
                            {
                                UserId = appUser.Id,
                                GroupId = group.Id
                            });
                        }
                        _appGroupService.AddUserToGroups(userGroups, appUser.Id);
                        _appGroupService.Save();
                        return request.CreateResponse(HttpStatusCode.OK, appUserViewModel);
                    }
                    return request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Join(",", result.Errors));
                }
                catch (NameDuplicatedException dex)
                {
                    return request.CreateErrorResponse(HttpStatusCode.BadRequest, dex.Message);
                }
            }
            return request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        [HttpDelete]
        [Route("delete")]
        [Authorize(Roles = "DeleteUser")]
        public async Task<HttpResponseMessage> Delete(HttpRequestMessage request, string id)
        {
            var appUser = await _userManager.FindByIdAsync(id);
            var result = await _userManager.DeleteAsync(appUser);
            if (result.Succeeded)
                return request.CreateResponse(HttpStatusCode.OK, id);
            return request.CreateErrorResponse(HttpStatusCode.OK, string.Join(",", result.Errors));
        }

        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public async Task<HttpResponseMessage> Login(HttpRequestMessage request, LoginViewModel user)
        {
            if (!ModelState.IsValid)
            {
                return request.CreateResponse(HttpStatusCode.OK, new {success = false});
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, user.Password, user.RememberMe, false);
            switch (result)
            {
                case SignInStatus.Success:
                    return request.CreateResponse(HttpStatusCode.OK, new {success = true});

                case SignInStatus.LockedOut:
                    return request.CreateResponse(HttpStatusCode.OK, new {success = false});

                case SignInStatus.Failure:
                default:
                    return request.CreateResponse(HttpStatusCode.OK, new {success = false});
            }
        }

        [HttpPost]
        [Authorize]
        [Route("logout")]
        public HttpResponseMessage LogOut(HttpRequestMessage request)
        {
            var authenticationManager = HttpContext.Current.GetOwinContext().Authentication;
            authenticationManager.SignOut();
            return request.CreateResponse(HttpStatusCode.OK, new {success = true});
        }
    }
}