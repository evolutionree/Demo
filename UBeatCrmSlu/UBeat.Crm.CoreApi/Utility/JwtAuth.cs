using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UBeat.Crm.CoreApi.Core.Utility;

namespace UBeat.Crm.CoreApi.Utility
{
    public class JwtAuth
    {
        private static readonly string Issue;
        private static readonly SecurityKey SignKey;
        private static readonly IConfigurationSection Config;

        static JwtAuth()
        {
            Config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("Jwt");
            var keyAsBytes = Encoding.ASCII.GetBytes(Config.GetValue<string>("SecretKey"));
            SignKey = new SymmetricSecurityKey(keyAsBytes);
            Issue = Config.GetValue<string>("Issuer");
        }

        public static JwtBearerOptions GetJwtOptions()
        {
            return new JwtBearerOptions
            {
                TokenValidationParameters =
                {
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Issue,
                    IssuerSigningKey = SignKey,
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                },
                Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = c =>
                    {
                        return Task.Run(() =>
                        {
                            
                            throw new UnauthorizedAccessException();
                            //Auth Fail
                        });
                    }
                }
            };
        }
        public static void GetJwtOptions(JwtBearerOptions o)
        {

            o.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issue,
                IssuerSigningKey = SignKey,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
            o.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = c =>
                {
                    return Task.Run(() =>
                    {

                        throw new UnauthorizedAccessException();
                        //Auth Fail
                    });
                }
            };

        }
        public static string SignToken(IList<Claim> claims,out DateTime expiration)
        {
            var seconds = Config.GetValue<int>("Expiration");
            expiration = DateTime.UtcNow.AddSeconds(seconds);
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(Issue, claims: claims,
                expires: expiration,
                signingCredentials: new SigningCredentials(SignKey, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }
    }
}