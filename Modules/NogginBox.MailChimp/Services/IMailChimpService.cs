using System;
using Orchard;
using NogginBox.MailChimp.Models;
using System.Collections.Generic;
using Orchard.ContentManagement;
using NogginBox.MailChimp.ViewModels;

namespace NogginBox.MailChimp.Services
{
	public interface IMailChimpService : IDependency
	{
		MailChimpSettingsPart MailChimpSettings { get; }
		Dictionary<String, String> CountryCodes { get; }
		Dictionary<String, String> getListsFromApi();
		List<InterestGroupingsRecord> getInterestGroupsFromApi(String listId);
		IList<MergeVariableRecord> getMergeVariablesFromApi(String listId);
		bool IsApiKeyValid(String apiKey);
		bool Subscribe(String listId, String subscribeEmail, Dictionary<String, object> mergeVars, String emailType, Dictionary<int, List<String>> interestGroups = null);
		void UpdateInterestGroupsForContentItem(MailChimpFormPart part, IEnumerable<int> showInterestGroupIds, IEnumerable<InterestGroupingsRecord> interestGroups);
		void UpdateMergeVariablesForContentItem(MailChimpFormPart part, IEnumerable<MergeVariableEntry> selectedMergeVars, IEnumerable<MergeVariableRecord> availableMergeVars);
	}
}