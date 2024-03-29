﻿using Cloud2.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Cloud2.Controllers.v1
{
    public class AccountsController : ApiController
    {
        private AuthRepository _repo = null;

        public AccountsController()
        {
            _repo = new AuthRepository();
        }

        //api/v1/accounts
        //Creates a new user with a new account.
        [AllowAnonymous]
        public async Task<IHttpActionResult> Post(UserModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await _repo.RegisterUser(userModel);

            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }
            using (var db = new MyDbContext())
            {
                User u = new User();
                u.firstname = userModel.firstname;
                u.lastname = userModel.lastname;
                u.email = userModel.email;
                u.username = userModel.UserName;
                db.Users.Add(u);
                db.SaveChanges();
            }
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
            }

            base.Dispose(disposing);
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}
