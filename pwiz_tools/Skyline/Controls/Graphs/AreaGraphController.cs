/*
 * Original author: Brendan MacLean <brendanx .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2010 University of Washington - Seattle, WA
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Linq;
using System.Windows.Forms;
using pwiz.Common.Controls;
using pwiz.Skyline.Model;
using pwiz.Skyline.Model.DocSettings;
using pwiz.Skyline.Properties;
using pwiz.Skyline.Util;

namespace pwiz.Skyline.Controls.Graphs
{
    public enum AreaNormalizeToView{ area_percent_view, area_maximum_view, area_ratio_view, area_global_standard_view, none }

    public enum AreaScope{ document, protein }

    public enum GraphTypePeakArea { replicate, peptide, histogram, histogram2d }

    public enum PointsTypePeakArea { targets, decoys }

    public enum AreaCVNormalizationMethod { global_standards, medians, none, ratio }

    public sealed class AreaGraphController : GraphSummary.IControllerSplit
    {
        public static GraphTypePeakArea GraphType
        {
            get { return Helpers.ParseEnum(Settings.Default.AreaGraphType, GraphTypePeakArea.replicate); }
            set { Settings.Default.AreaGraphType = value.ToString(); }
        }

        public static AreaNormalizeToView AreaView
        {
            get { return Helpers.ParseEnum(Settings.Default.AreaNormalizeToView, AreaNormalizeToView.none); }
            set { Settings.Default.AreaNormalizeToView = value.ToString(); }
        }

        public static AreaScope AreaScope
        {
            get { return Helpers.ParseEnum(Settings.Default.PeakAreaScope, AreaScope.document); }
            set { Settings.Default.PeakAreaScope = value.ToString(); }
        }

        public static PointsTypePeakArea PointsType
        {
            get { return Helpers.ParseEnum(Settings.Default.AreaCVPointsType, PointsTypePeakArea.targets); }
            set { Settings.Default.AreaCVPointsType = value.ToString(); }
        }

        public static AreaCVNormalizationMethod NormalizationMethod
        {
            get { return Helpers.ParseEnum(Settings.Default.AreaCVNormalizationMethod, AreaCVNormalizationMethod.none); }
            set { Settings.Default.AreaCVNormalizationMethod = value.ToString(); }
        }

        public static string GroupByGroup { get; set; }
        public static string GroupByAnnotation { get; set; }

        public static int MinimumDetections = 2;

        public static int AreaCVRatioIndex = -1;

        public GraphSummary GraphSummary { get; set; }

        public IFormView FormView { get { return new GraphSummary.AreaGraphView(); } }

        public static double GetAreaCVFactorToDecimal()
        {
            return Settings.Default.AreaCVShowDecimals ? 1.0 : 100.0;
        }

        public static double GetAreaCVFactorToPercentage()
        {
            return Settings.Default.AreaCVShowDecimals ? 100.0 : 1.0;
        }

        public static bool ShouldUseQValues(SrmDocument document)
        {
            return PointsType == PointsTypePeakArea.targets &&
                document.Settings.PeptideSettings.Integration.PeakScoringModel.IsTrained &&
                !double.IsNaN(Settings.Default.AreaCVQValueCutoff) &&
                Settings.Default.AreaCVQValueCutoff < 1.0;
        }

        public void OnDocumentChanged(SrmDocument oldDocument, SrmDocument newDocument)
        {
            var settingsNew = newDocument.Settings;
            var settingsOld = oldDocument.Settings;

            if (GraphType == GraphTypePeakArea.histogram || GraphType == GraphTypePeakArea.histogram2d)
            {
                if (GroupByGroup != null && !ReferenceEquals(settingsNew.DataSettings.AnnotationDefs, settingsOld.DataSettings.AnnotationDefs))
                {
                    var groups = AnnotationHelper.FindGroupsByTarget(settingsNew, AnnotationDef.AnnotationTarget.replicate);
                    // The group we were grouping by has been removed
                    if (!groups.Contains(GroupByGroup))
                    {
                        GroupByGroup = GroupByAnnotation = null;
                    }
                }

                if (GroupByAnnotation != null && settingsNew.HasResults && settingsOld.HasResults &&
                    !ReferenceEquals(settingsNew.MeasuredResults.Chromatograms, settingsOld.MeasuredResults.Chromatograms))
                {
                    var annotations = AnnotationHelper.GetPossibleAnnotations(settingsNew, GroupByGroup, AnnotationDef.AnnotationTarget.replicate);

                    // The annotation we were grouping by has been removed
                    if (!annotations.Contains(GroupByAnnotation))
                        GroupByAnnotation = null;

                    var pane = GraphSummary.GraphPanes.FirstOrDefault();
                    if (pane is AreaCVHistogramGraphPane)
                        ((AreaCVHistogramGraphPane) pane).Cache.Cancel();
                    else if(pane is AreaCVHistogram2DGraphPane)
                        ((AreaCVHistogram2DGraphPane) pane).Cache.Cancel();
                }
            }
        }

        public void OnActiveLibraryChanged()
        {
            if (GraphSummary.GraphPanes.OfType<AreaReplicateGraphPane>().Any())
                GraphSummary.UpdateUI();
        }

        public void OnResultsIndexChanged()
        {
            if (GraphSummary.GraphPanes.OfType<AreaReplicateGraphPane>().Any() /* || !Settings.Default.AreaAverageReplicates */ ||
                    RTLinearRegressionGraphPane.ShowReplicate == ReplicateDisplay.single)
                GraphSummary.UpdateUI();
        }

        public void OnRatioIndexChanged()
        {
            if (GraphSummary.GraphPanes.OfType<AreaReplicateGraphPane>().Any() /* || !Settings.Default.AreaAverageReplicates */)
                GraphSummary.UpdateUI();
        }

        public void OnUpdateGraph()
        {
            // CONSIDER: Need a better guarantee that this ratio index matches the
            //           one in the sequence tree, but at least this will keep the UI
            //           from crashing with IndexOutOfBoundsException.
            var settings = GraphSummary.DocumentUIContainer.DocumentUI.Settings;
            var mods = settings.PeptideSettings.Modifications;
            GraphSummary.RatioIndex = Math.Min(GraphSummary.RatioIndex, mods.RatioInternalStandardTypes.Count - 1);

            // Only show ratios if document changes to have valid ratios
            if (AreaView == AreaNormalizeToView.area_ratio_view && !mods.HasHeavyModifications)
                AreaView = AreaNormalizeToView.none;

            // Only ratios if type and info match
            if (NormalizationMethod == AreaCVNormalizationMethod.ratio && !mods.HasHeavyModifications ||
                NormalizationMethod == AreaCVNormalizationMethod.global_standards && !settings.HasGlobalStandardArea)
            {
                NormalizationMethod = AreaCVNormalizationMethod.none;
            }

            AreaCVRatioIndex = Math.Min(AreaCVRatioIndex, mods.RatioInternalStandardTypes.Count - 1);

            var globalStandards = NormalizationMethod == AreaCVNormalizationMethod.global_standards;
            if (globalStandards && !GraphSummary.DocumentUIContainer.DocumentUI.Settings.HasGlobalStandardArea)
                NormalizationMethod = AreaCVNormalizationMethod.none;

            var pane = GraphSummary.GraphPanes.FirstOrDefault();

            switch (GraphType)
            {
                case GraphTypePeakArea.replicate:
                case GraphTypePeakArea.peptide:
                    GraphSummary.DoUpdateGraph(this, (GraphTypeSummary) Enum.Parse(typeof(GraphTypeSummary), GraphType.ToString()));
                    break;
                case GraphTypePeakArea.histogram:
                    if (!(pane is AreaCVHistogramGraphPane))
                        GraphSummary.GraphPanes = new[] { new AreaCVHistogramGraphPane(GraphSummary) };
                    break;
                case GraphTypePeakArea.histogram2d:
                    if (!(pane is AreaCVHistogram2DGraphPane))
                        GraphSummary.GraphPanes = new[] { new AreaCVHistogram2DGraphPane(GraphSummary)  };
                    break;
            }

            if (!ReferenceEquals(GraphSummary.GraphPanes.FirstOrDefault(), pane))
            {
                var disposable = pane as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }

        public bool IsReplicatePane(SummaryGraphPane pane)
        {
            return pane is AreaReplicateGraphPane;
        }

        public bool IsPeptidePane(SummaryGraphPane pane)
        {
            return pane is AreaPeptideGraphPane;
        }

        public SummaryGraphPane CreateReplicatePane(PaneKey key)
        {
            return new AreaReplicateGraphPane(GraphSummary, key);
        }

        public SummaryGraphPane CreatePeptidePane(PaneKey key)
        {
            return new AreaPeptideGraphPane(GraphSummary, key);
        }

        public bool HandleKeyDownEvent(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
//                case Keys.D3:
//                    if (e.Alt)
//                        GraphSummary.Hide();
//                    break;
                case Keys.F7:
                    if (!e.Alt && !(e.Shift && e.Control))
                    {
                        if (e.Control)
                            Settings.Default.AreaGraphType = GraphTypeSummary.peptide.ToString();
                        else
                            Settings.Default.AreaGraphType = GraphTypeSummary.replicate.ToString();
                        GraphSummary.UpdateUI();
                    }
                    break;
            }
            return false;
        }
    }
}
