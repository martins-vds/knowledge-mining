using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using KnowledgeMining.UI;
using KnowledgeMining.UI.Shared;
using MudBlazor;
using KnowledgeMining.UI.Services.Search;
using KnowledgeMining.UI.Services.Storage;
using KnowledgeMining.UI.Extensions;
using KnowledgeMining.UI.Options;
using KnowledgeMining.UI.Pages.Documents.Componenents;
using KnowledgeMining.UI.Services.Storage.Models;

namespace KnowledgeMining.UI.Pages.Documents
{
    public partial class Index
    {
        [Inject] public ISnackbar Snackbar { get; set; }
        [Inject] public IStorageService StorageService { get; set; }
        [Inject] public ISearchService SearchService { get; set; }
        [Inject] public IDialogService DialogService { get; set; }

        private bool _isLoading;
        private string? _searchText;
        private string? _currentPage;
        private string? _nextPage;
        private int _pageSize = 10;
        private Document _backupSelectedDocument;
        private IEnumerable<Document> _documents = new List<Document>();
        private Stack<string?> _pageHistory = new Stack<string?>();
        // Upload Document
        private bool _isUploadComponentVisible;
        public void OpenUploadComponent()
        {
            _isUploadComponentVisible = true;
        }

        protected override async Task OnInitializedAsync()
        {
            await Search(_searchText);
        }

        private void BackupDocument(Document item)
        {
            _backupSelectedDocument = new Document(item.Name, item.Tags);
        }

        private void RestoreDocument(Document item)
        {
            item = _backupSelectedDocument;
        }

        private void SaveDocument(Document item)
        {
        // TODO: Save changes
        }

        private async ValueTask DeleteDocument(Document document)
        {
            var parameters = new DialogParameters{["document"] = document};
            var dialog = DialogService.Show<DeleteDocumentDialogComponent>("Delete Document", parameters);
            var result = await dialog.Result;
            if (result.Cancelled)
            {
                return;
            }

            try
            {
                await StorageService.DeleteDocument(document.Name, CancellationToken.None);
                await SearchService.QueueIndexerJob(CancellationToken.None);
                await Search(_searchText);
                StateHasChanged();
                Snackbar.Add("Document deleted", Severity.Success);
            }
            catch
            {
                Snackbar.Add("Failed to deleted Document", Severity.Error);
            }
        }

        private async Task OnSearch(string searchText)
        {
            _searchText = searchText;
            await Search(searchText);
        }

        private async Task LoadPreviousPage()
        {
            var previousPage = GetLastPageFromHistory();
            await Search(_searchText, previousPage);
        }

        private async Task LoadNextPage()
        {
            AddPageToHistory(_currentPage);
            await Search(_searchText, _nextPage);
        }

        private async Task Search(string? searchText, string? nextPage = default)
        {
            SearchDocumentsResponse? response;
            _isLoading = true;
            response = await StorageService.SearchDocuments(searchText, _pageSize, nextPage, CancellationToken.None);
            _isLoading = false;
            if (string.IsNullOrWhiteSpace(nextPage))
            {
                CleanPageHistory();
            }

            UpdateTable(response.Documents);
            SetCurrentPage(nextPage);
            SetNextPage(response.NextPage);
        }

        private void UpdateTable(IEnumerable<Document> documents)
        {
            _documents = documents;
        }

        private void AddPageToHistory(string? page)
        {
            _pageHistory.Push(page);
        }

        private string? GetLastPageFromHistory()
        {
            return _pageHistory.Any() ? _pageHistory.Pop() : default;
        }

        private void CleanPageHistory()
        {
            if (_pageHistory.Any())
            {
                _pageHistory.Clear();
            }

            _nextPage = default;
        }

        private void SetCurrentPage(string? page)
        {
            _currentPage = page;
        }

        private void SetNextPage(string? page)
        {
            _nextPage = page;
        }
    }
}