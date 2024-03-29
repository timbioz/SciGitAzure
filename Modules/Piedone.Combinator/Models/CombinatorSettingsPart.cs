﻿using Orchard.ContentManagement;
using Orchard.Core.Common.Utilities;
using Orchard.Environment.Extensions;

namespace Piedone.Combinator.Models
{
    [OrchardFeature("Piedone.Combinator")]
    public class CombinatorSettingsPart : ContentPart<CombinatorSettingsPartRecord>
    {
        public string CombinationExcludeRegex
        {
            get { return Record.CombinationExcludeRegex; }
            set { Record.CombinationExcludeRegex = value; }
        }

        public bool CombineCdnResources
        {
            get { return Record.CombineCdnResources; }
            set { Record.CombineCdnResources = value; }
        }

        public string ResourceDomain
        {
            get { return Record.ResourceDomain; }
            set { Record.ResourceDomain = value; }
        }

        public bool EnableForAdmin
        {
            get { return Record.EnableForAdmin; }
            set { Record.EnableForAdmin = value; }
        }

        public bool MinifyResources
        {
            get { return Record.MinifyResources; }
            set { Record.MinifyResources = value; }
        }

        public string MinificationExcludeRegex
        {
            get { return Record.MinificationExcludeRegex; }
            set { Record.MinificationExcludeRegex = value; }
        }

        public bool EmbedCssImages
        {
            get { return Record.EmbedCssImages; }
            set { Record.EmbedCssImages = value; }
        }

        public int EmbeddedImagesMaxSizeKB
        {
            get { return Record.EmbeddedImagesMaxSizeKB; }
            set { Record.EmbeddedImagesMaxSizeKB = value; }
        }

        public string EmbedCssImagesStylesheetExcludeRegex
        {
            get { return Record.EmbedCssImagesStylesheetExcludeRegex; }
            set { Record.EmbedCssImagesStylesheetExcludeRegex = value; }
        }

        public bool GenerateImageSprites
        {
            get { return Record.GenerateImageSprites; }
            set { Record.GenerateImageSprites = value; }
        }

        public string ResourceSetRegexes
        {
            get { return Record.ResourceSetRegexes; }
            set { Record.ResourceSetRegexes = value; }
        }

        private readonly LazyField<int> _cacheFileCount = new LazyField<int>();
        public LazyField<int> CacheFileCountField { get { return _cacheFileCount; } }
        public int CacheFileCount
        {
            get { return _cacheFileCount.Value; }
        }
    }
}