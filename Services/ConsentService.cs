using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using mvcCookieAuthSample.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mvcCookieAuthSample.Services
{
    public class ConsentService
    {
        private readonly IClientStore _clientStore;
        private readonly IResourceStore _resourceStore;
        private readonly IIdentityServerInteractionService _identityServerInteractionService;


        public ConsentService(
            IClientStore clientStore,
            IResourceStore resourceStore,
            IIdentityServerInteractionService identityServerInteractionService)
        {
            _clientStore = clientStore;
            _resourceStore = resourceStore;
            _identityServerInteractionService = identityServerInteractionService;
        }

        #region private Methods
        
        private ConsentViewModel CreateConsentViewModel(AuthorizationRequest request, Client client, Resources resources,InputConsentViewModel model)
        {
            var rememberConsent = model?.RememberConsent ?? true;
            var selectedScopes = model?.ScopesConsented?? Enumerable.Empty<string>();
            var vm = new ConsentViewModel();
            vm.ClientName = client.ClientName;
            vm.ClientLogoUrl = client.LogoUri;
            vm.ClientUrl = client.ClientUri;            
            vm.RememberConsent = rememberConsent;


            vm.IdentityScorpes = resources.IdentityResources.Select(i => CreatScopeViewModel(i,selectedScopes.Contains(i.Name)||model==null));
            vm.ResourceScorpes = resources.ApiResources.SelectMany(i => i.Scopes).Select(x => CreatScopeViewModel(x, selectedScopes.Contains(x.Name) || model==null));
            return vm;
        }

        private ScopeViewModel CreatScopeViewModel(IdentityResource identityResource,bool check)
        {
            return new ScopeViewModel
            {
                Name = identityResource.Name,
                DisplayName = identityResource.DisplayName,
                Description = identityResource.Description,
                Checked = check || identityResource.Required,
                Required = identityResource.Required,
                Emphasize = identityResource.Emphasize
            };
        }

        private ScopeViewModel CreatScopeViewModel(Scope scope,bool check)
        {
            return new ScopeViewModel
            {
                Name = scope.Name,
                DisplayName = scope.DisplayName,
                Description = scope.Description,
                Checked = check || scope.Required,
                Required = scope.Required,
                Emphasize = scope.Emphasize
            };
        }
        #endregion

        public async Task<ConsentViewModel> BulidConsentViewModel(string returnUrl,InputConsentViewModel model=null)
        {
            
            var request = await _identityServerInteractionService.GetAuthorizationContextAsync(returnUrl);
            if (request == null)
                return null;
            var client = await _clientStore.FindEnabledClientByIdAsync(request.ClientId);
            var resoureces = await _resourceStore.FindEnabledResourcesByScopeAsync(request.ScopesRequested);
            
            var vm = CreateConsentViewModel(request, client, resoureces,model);
            vm.ReturnUrl = returnUrl;
            return vm;
        }

        public async Task<ProcessConsentResult> ProcessConent(InputConsentViewModel viewModel)
        {
            ConsentResponse consentResponse = null;
            var result = new ProcessConsentResult();
            if (viewModel.Button == "no")
            {
                consentResponse = ConsentResponse.Denied;
            }
            else if (viewModel.Button == "yes")
            {
                if (viewModel.ScopesConsented != null && viewModel.ScopesConsented.Any())
                {
                    consentResponse = new ConsentResponse
                    {
                        RememberConsent = viewModel.RememberConsent,
                        ScopesConsented = viewModel.ScopesConsented
                    };
                }

                result.ValidationError = "请至少选中一个权限";
            }

            if (consentResponse != null)
            {                
                var request = await _identityServerInteractionService.GetAuthorizationContextAsync(viewModel.ReturnUrl);
                await _identityServerInteractionService.GrantConsentAsync(request, consentResponse);

                result.RedirectUrl = viewModel.ReturnUrl;                
            }

            {
                var consentfViewModel = await BulidConsentViewModel(viewModel.ReturnUrl,viewModel);
                result.ViewModel = consentfViewModel;
            }
            return result;
        }

    }
}
