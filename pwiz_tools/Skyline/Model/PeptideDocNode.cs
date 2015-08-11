/*
 * Original author: Brendan MacLean <brendanx .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2009 University of Washington - Seattle, WA
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using pwiz.Skyline.Controls.SeqNode;
using pwiz.Skyline.Model.DocSettings;
using pwiz.Skyline.Model.Lib;
using pwiz.Skyline.Model.Results;
using pwiz.Skyline.Model.Results.Scoring;
using pwiz.Skyline.Properties;
using pwiz.Skyline.Util;

namespace pwiz.Skyline.Model
{
    public class PeptideDocNode : DocNodeParent
    {
        public const string STANDARD_TYPE_IRT = "iRT";  // Not L10N
        public const string STANDARD_TYPE_QC = "QC";    // Not L10N
        public const string STANDARD_TYPE_NORMALIZAITON = "Normalization";  // Not L10N

        public static string GetStandardTypeDisplayName(string standardType)
        {
            switch (standardType)
            {
                case STANDARD_TYPE_IRT:
                    return Resources.PeptideDocNode_GetStandardTypeDisplayName_iRT;
                case STANDARD_TYPE_QC:
                    return Resources.PeptideDocNode_GetStandardTypeDisplayName_QC;
                case STANDARD_TYPE_NORMALIZAITON:
                    return Resources.PeptideDocNode_GetStandardTypeDisplayName_Normalization;
            }
            return string.Empty;
        }

        public PeptideDocNode(Peptide id, ExplicitMods mods = null, ExplicitRetentionTimeInfo explicitRetentionTime = null)
            : this(id, null, mods, null, null, null, explicitRetentionTime, Annotations.EMPTY, null, new TransitionGroupDocNode[0], true)
        {
        }

        public PeptideDocNode(Peptide id, SrmSettings settings, ExplicitMods mods, ModifiedSequenceMods sourceKey, ExplicitRetentionTimeInfo explicitRetentionTime,
            TransitionGroupDocNode[] children, bool autoManageChildren)
            : this(id, settings, mods, sourceKey, null, null, explicitRetentionTime, Annotations.EMPTY, null, children, autoManageChildren)
        {
        }

        public PeptideDocNode(Peptide id,
                              SrmSettings settings,
                              ExplicitMods mods,
                              ModifiedSequenceMods sourceKey,
                              string standardType,
                              int? rank,
                              ExplicitRetentionTimeInfo explicitRetentionTimeInfo,
                              Annotations annotations,
                              Results<PeptideChromInfo> results,
                              TransitionGroupDocNode[] children,
                              bool autoManageChildren)
            : base(id, annotations, children, autoManageChildren)
        {
            ExplicitMods = mods;
            SourceKey = sourceKey;
            GlobalStandardType = standardType;
            Rank = rank;
            ExplicitRetentionTime = explicitRetentionTimeInfo;
            Results = results;
            BestResult = CalcBestResult();

            if (settings != null)
            {
                var calcPre = settings.GetPrecursorCalc(IsotopeLabelType.light, ExplicitMods);
                ModifiedSequence = calcPre.GetModifiedSequence(Peptide.Sequence, false);
                ModifiedSequenceDisplay = calcPre.GetModifiedSequence(Peptide.Sequence, true);
            }
        }

        public Peptide Peptide { get { return (Peptide)Id; } }

        public PeptideModKey Key { get { return new PeptideModKey(Peptide, ExplicitMods); } }

        public PeptideSequenceModKey SequenceKey { get { return new PeptideSequenceModKey(Peptide, ExplicitMods); } }

        public override AnnotationDef.AnnotationTarget AnnotationTarget { get { return AnnotationDef.AnnotationTarget.peptide; } }

        public ExplicitMods ExplicitMods { get; private set; }

        public ModifiedSequenceMods SourceKey { get; private set; }

        public ExplicitRetentionTimeInfo ExplicitRetentionTime { get; private set; } // For transition lists with explicit values for RT

        public string GlobalStandardType { get; private set; }

        public string ModifiedSequence { get; private set; }

        public string ModifiedSequenceDisplay { get; private set; }

        public string RawTextId { get { return CustomInvariantNameOrText(ModifiedSequence); } }

        public string RawUnmodifiedTextId { get { return CustomInvariantNameOrText(Peptide.Sequence); }}

        public string RawTextIdDisplay { get { return CustomDisplayNameOrText(ModifiedSequenceDisplay); } }

        private string CustomDisplayNameOrText(string text)
        {
            return Peptide.IsCustomIon ? Peptide.CustomIon.DisplayName : text;
        }

        private string CustomInvariantNameOrText(string text)
        {
            return Peptide.IsCustomIon ? Peptide.CustomIon.InvariantName : text;
        }

        /// <summary>
        /// For decoy peptides, returns modified sequence of the source peptide
        /// For non-decoy peptides, returns modified sequence
        /// For non-peptides, returns the display name (Name or Formula or masses)
        /// </summary>
        public string SourceTextId { get { return SourceKey != null ? SourceKey.ModifiedSequence : RawTextId; } }

        /// <summary>
        /// For decoy peptides, returns unmodified sequence of the source peptide
        /// For non-decoy peptides, returns unmodified sequence
        /// For non-peptides, returns the display name (Name or Formula or masses)
        /// </summary>
        public string SourceUnmodifiedTextId { get { return SourceKey != null ? SourceKey.Sequence : RawUnmodifiedTextId; } }

        /// <summary>
        /// Explicit modifications for this peptide or a source peptide for a decoy
        /// Combined with SourceUnmodifiedTextId to form a unique key
        /// </summary>
        public ExplicitMods SourceExplicitMods { get { return SourceKey != null ? SourceKey.ExplicitMods : ExplicitMods; } }

        public bool HasExplicitMods { get { return ExplicitMods != null; } }

        public bool HasVariableMods { get { return HasExplicitMods && ExplicitMods.IsVariableStaticMods; } }

        public bool IsProteomic { get { return !Peptide.IsCustomIon; } }

        public bool AreVariableModsPossible(int maxVariableMods, IList<StaticMod> modsVarAvailable)
        {
            if (HasVariableMods)
            {
                var explicitModsVar = ExplicitMods.StaticModifications;
                if (explicitModsVar.Count > maxVariableMods)
                    return false;
                foreach (var explicitMod in explicitModsVar)
                {
                    if (!modsVarAvailable.Contains(explicitMod.Modification))
                        return false;
                }
            }
            return true;
        }

        public bool CanHaveImplicitStaticMods
        {
            get
            {
                return !HasExplicitMods ||
                       !ExplicitMods.IsModified(IsotopeLabelType.light) ||
                       ExplicitMods.IsVariableStaticMods;
            }
        }

        public bool CanHaveImplicitHeavyMods(IsotopeLabelType labelType)
        {
            return !HasExplicitMods || !ExplicitMods.IsModified(labelType);
        }

        public bool HasChildType(IsotopeLabelType labelType)
        {
            return Children.Contains(nodeGroup => ReferenceEquals(labelType,
                                                                  ((TransitionGroupDocNode)nodeGroup).TransitionGroup.LabelType));
        }

        public bool HasChildCharge(int charge)
        {
            return Children.Contains(nodeGroup => Equals(charge,
                                                         ((TransitionGroupDocNode) nodeGroup).TransitionGroup.PrecursorCharge));
        }

        public int? Rank { get; private set; }
        public bool IsDecoy { get { return Peptide.IsDecoy; } }

        public Results<PeptideChromInfo> Results { get; private set; }

        public bool HasResults { get { return Results != null; } }

        public ChromInfoList<PeptideChromInfo> GetSafeChromInfo(int i)
        {
            return (HasResults && Results.Count > i ? Results[i] : null);
        }

        public float GetRankValue(PeptideRankId rankId)
        {
            float value = float.MinValue;
            foreach (TransitionGroupDocNode nodeGroup in Children)
                value = Math.Max(value, nodeGroup.GetRankValue(rankId));
            return value;
        }

        public float? GetPeakCountRatio(int i)
        {
            if (i == -1)
                return AveragePeakCountRatio;

            var result = GetSafeChromInfo(i);
            if (result == null)
                return null;
            return result.GetAverageValue(chromInfo => chromInfo.PeakCountRatio);
        }

        public float? AveragePeakCountRatio
        {
            get
            {
                return GetAverageResultValue(chromInfo => chromInfo.PeakCountRatio);
            }
        }

        public float? GetSchedulingTime(int i)
        {
            return GetMeasuredRetentionTime(i);
        }

        public float? SchedulingTime
        {
            get { return AverageMeasuredRetentionTime; }
        }

        public float? GetSchedulingTime(ChromFileInfoId fileId)
        {
            return GetMeasuredRetentionTime(fileId);
        }

        public float? GetMeasuredRetentionTime(int i)
        {
            if (i == -1)
                return AverageMeasuredRetentionTime;

            var result = GetSafeChromInfo(i);
            if (result == null)
                return null;
            return result.GetAverageValue(chromInfo => chromInfo.RetentionTime.HasValue
                                             ? chromInfo.RetentionTime.Value
                                             : (float?)null);
        }

        public float? AverageMeasuredRetentionTime
        {
            get
            {
                return GetAverageResultValue(chromInfo => chromInfo.RetentionTime.HasValue
                                             ? chromInfo.RetentionTime.Value
                                             : (float?)null);
            }
        }

        public float? GetMeasuredRetentionTime(ChromFileInfoId fileId)
        {
            double totalTime = 0;
            int countTime = 0;
            foreach (var chromInfo in TransitionGroups.SelectMany(nodeGroup => nodeGroup.ChromInfos))
            {
                if (fileId != null && !ReferenceEquals(fileId, chromInfo.FileId))
                    continue;
                float? retentionTime = chromInfo.RetentionTime;
                if (!retentionTime.HasValue)
                    continue;

                totalTime += retentionTime.Value;
                countTime++;
            }
            if (countTime == 0)
                return null;
            return (float)(totalTime / countTime);
        }

        public float? GetPeakCenterTime(int i)
        {
            if (i == -1)
                return AveragePeakCenterTime;

            double totalTime = 0;
            int countTime = 0;
            foreach (var nodeGroup in TransitionGroups)
            {
                if (!nodeGroup.HasResults)
                    continue;
                var result = nodeGroup.Results[i];
                if (result == null)
                    continue;

                foreach (var chromInfo in result)
                {
                    float? centerTime = GetPeakCenterTime(chromInfo);
                    if (!centerTime.HasValue)
                        continue;

                    totalTime += centerTime.Value;
                    countTime++;
                }
            }
            if (countTime == 0)
                return null;
            return (float)(totalTime / countTime);
        }

        public float? AveragePeakCenterTime
        {
            get { return GetPeakCenterTime((ChromFileInfoId) null); }
        }

        public float? GetPeakCenterTime(ChromFileInfoId fileId)
        {
            double totalTime = 0;
            int countTime = 0;
            foreach (var chromInfo in TransitionGroups.SelectMany(nodeGroup => nodeGroup.ChromInfos))
            {
                if (fileId != null && !ReferenceEquals(fileId, chromInfo.FileId))
                    continue;
                float? centerTime = GetPeakCenterTime(chromInfo);
                if (!centerTime.HasValue)
                    continue;

                totalTime += centerTime.Value;
                countTime++;
            }
            if (countTime == 0)
                return null;
            return (float)(totalTime / countTime);
        }

        private float? GetPeakCenterTime(TransitionGroupChromInfo chromInfo)
        {
            if (!chromInfo.StartRetentionTime.HasValue || !chromInfo.EndRetentionTime.HasValue)
                return null;

            return (chromInfo.StartRetentionTime.Value + chromInfo.EndRetentionTime.Value) / 2;
        }

        private float? GetAverageResultValue(Func<PeptideChromInfo, float?> getVal)
        {
            return HasResults ? Results.GetAverageValue(getVal) : null;
        }

        public int BestResult { get; private set; }

        /// <summary>
        /// Returns the index of the "best" result for a peptide.  This is currently
        /// base solely on total peak area, could be enhanced in the future to be
        /// more like picking the best peak in the import code, including factors
        /// such as peak-found-ratio and dot-product.
        /// </summary>
        private int CalcBestResult()
        {
            if (!HasResults)
                return -1;

            int iBest = -1;
            double bestArea = double.MinValue;
            for (int i = 0; i < Results.Count; i++)
            {
                double combinedScore = 0;
                foreach (TransitionGroupDocNode nodeGroup in Children)
                {
                    double groupArea = 0;
                    double groupTranMeasured = 0;
                    bool isGroupIdentified = false;
                    foreach (TransitionDocNode nodeTran in nodeGroup.Children)
                    {
                        if (!nodeTran.HasResults)
                            continue;
                        var result = nodeTran.Results[i];
                        if (result == null)
                            continue;
                        // Use average area over all files in a replicate to avoid
                        // counting a replicate as best, simply because it has more
                        // measurements.  Most of the time there should only be one
                        // file per precursor per replicate.
                        double tranArea = 0;
                        double tranMeasured = 0;
                        foreach (var chromInfo in result)
                        {
                            if (chromInfo != null && chromInfo.Area > 0)
                            {
                                tranArea += chromInfo.Area;
                                tranMeasured++;

                                isGroupIdentified = isGroupIdentified || chromInfo.IsIdentified;
                            }
                        }
                        groupArea += tranArea/result.Count;
                        groupTranMeasured += tranMeasured/result.Count;
                    }
                    combinedScore += ChromDataPeakList.ScorePeak(groupArea,
                        LegacyCountScoreCalc.GetPeakCountScore(groupTranMeasured, nodeGroup.Children.Count),
                        isGroupIdentified);
                }
                if (combinedScore > bestArea)
                {
                    iBest = i;
                    bestArea = combinedScore;
                }
            }
            return iBest;            
        }

        #region Property change methods

        private PeptideDocNode UpdateModifiedSequence(SrmSettings settingsNew)
        {
            if (!IsProteomic)
                return this; // Settings have no effect on custom ions

            var calcPre = settingsNew.GetPrecursorCalc(IsotopeLabelType.light, ExplicitMods);
            string modifiedSequence = calcPre.GetModifiedSequence(Peptide.Sequence, false);
            string modifiedSequenceDisplay = calcPre.GetModifiedSequence(Peptide.Sequence, true);
            if (string.Equals(modifiedSequence, ModifiedSequence) &&
                string.Equals(modifiedSequenceDisplay, ModifiedSequenceDisplay))
            {
                return this;
            }
            return ChangeProp(ImClone(this), im =>
                {
                    im.ModifiedSequence = modifiedSequence;
                    im.ModifiedSequenceDisplay = modifiedSequenceDisplay;
                });
        }

        public PeptideDocNode ChangeExplicitMods(ExplicitMods prop)
        {
            return ChangeProp(ImClone(this), im => im.ExplicitMods = prop);
        }

        public PeptideDocNode ChangeSourceKey(ModifiedSequenceMods prop)
        {
            return ChangeProp(ImClone(this), im => im.SourceKey = prop);
        }

        public PeptideDocNode ChangeStandardType(string prop)
        {
            return ChangeProp(ImClone(this), im => im.GlobalStandardType = prop);
        }

        public PeptideDocNode ChangeRank(int? prop)
        {
            return ChangeProp(ImClone(this), im => im.Rank = prop);
        }

        public PeptideDocNode ChangeResults(Results<PeptideChromInfo> prop)
        {
            return ChangeProp(ImClone(this), im =>
                                                 {
                                                     im.Results = prop;
                                                     im.BestResult = im.CalcBestResult();
                                                 });
        }

        public PeptideDocNode ChangeExplicitRetentionTime(ExplicitRetentionTimeInfo prop)
        {
            return ChangeProp(ImClone(this), im => im.ExplicitRetentionTime = prop);
        }

        public PeptideDocNode ChangeExplicitRetentionTime(double? prop)
        {
            double? oldwindow = null;
            if (ExplicitRetentionTime != null)
                oldwindow = ExplicitRetentionTime.RetentionTimeWindow;
            return ChangeProp(ImClone(this), im => im.ExplicitRetentionTime = prop.HasValue ? new ExplicitRetentionTimeInfo(prop.Value, oldwindow) : null);
        }

        // Note: this potentially returns a node with a different ID, which has to be Inserted rather than Replaced
        public PeptideDocNode ChangeCustomIonValues(SrmSettings settings, DocNodeCustomIon customIon, int charge, ExplicitRetentionTimeInfo explicitRetentionTime, ExplicitTransitionGroupValues explicitTransitionGroupValues)
        {
            Assume.IsTrue(TransitionGroupCount == 1);  // We support just one transition group per custom molecule
            TransitionGroupDocNode nodeGroup = TransitionGroups.First();
            if (explicitTransitionGroupValues == null)
                explicitTransitionGroupValues = ExplicitTransitionGroupValues.EMPTY;
            var newPeptide = new Peptide(customIon);
            Helpers.AssignIfEquals(ref newPeptide, Peptide);
            var group = new TransitionGroup(newPeptide, charge,
                nodeGroup.TransitionGroup.LabelType, true, nodeGroup.TransitionGroup.DecoyMassShift);
            if (Equals(group, nodeGroup.TransitionGroup))
            {
                if (!Equals(explicitTransitionGroupValues, nodeGroup.ExplicitValues))
                    nodeGroup = nodeGroup.ChangeExplicitValues(explicitTransitionGroupValues);
                return (PeptideDocNode) ChangeExplicitRetentionTime(explicitRetentionTime).ChangeChildrenChecked(new[] {nodeGroup});
            }
            else
            {
                // ID Changes impact all children, because IDs have back pointers to their parents
                var children = new List<TransitionDocNode>();
                foreach (var nodeTran in nodeGroup.Transitions)
                {
                    var transition = nodeTran.Transition;
                    var tranNew = new Transition(group, transition.IonType, transition.CleavageOffset,
                        transition.MassIndex, transition.Charge, transition.DecoyMassShift, transition.CustomIon);
                    var nodeTranNew = new TransitionDocNode(tranNew, nodeTran.Annotations, nodeTran.Losses,
                        nodeTran.GetIonMass(), nodeTran.IsotopeDistInfo, nodeTran.LibInfo, nodeTran.Results);
                    children.Add(nodeTranNew);
                }
                var newNodeGroup = new TransitionGroupDocNode(group, nodeGroup.Annotations, settings, ExplicitMods, nodeGroup.LibInfo,
                    explicitTransitionGroupValues, nodeGroup.Results,
                    children.ToArray(), AutoManageChildren);
                return new PeptideDocNode(newPeptide, settings, ExplicitMods, SourceKey, explicitRetentionTime, new[] { newNodeGroup }, AutoManageChildren);
            }
        }

        #endregion

        /// <summary>
        /// Node level depths below this node
        /// </summary>
// ReSharper disable InconsistentNaming
        public enum Level { TransitionGroups, Transitions }
// ReSharper restore InconsistentNaming

        public int TransitionGroupCount { get { return GetCount((int)Level.TransitionGroups); } }
        public int TransitionCount { get { return GetCount((int)Level.Transitions); } }

        public IEnumerable<TransitionGroupDocNode> TransitionGroups
        {
            get { return Children.Cast<TransitionGroupDocNode>(); }
        }

        public bool HasHeavyTransitionGroups
        {
            get
            {
                return TransitionGroups.Contains(nodeGroup => !nodeGroup.TransitionGroup.LabelType.IsLight);
            }
        }

        public bool HasLibInfo
        {
            get
            {
                return TransitionGroups.Contains(nodeGroup => nodeGroup.HasLibInfo);
            }
        }

        public bool IsUserModified
        {
            get
            {
                if (!Annotations.IsEmpty)
                    return true;
                return TransitionGroups.Contains(nodeGroup => nodeGroup.IsUserModified);
            }
        }

        /// <summary>
        /// Given a <see cref="TransitionGroupDocNode"/> returns a <see cref="TransitionGroupDocNode"/> for which
        /// transition rankings based on imported results should be used for determining primary transitions
        /// in triggered-MRM (iSRM).  This ensures that light and isotope labeled precursors with the same
        /// transitions use the same ranking, and that only one isotope label type need be measured to
        /// produce a method for a document with light-heavy pairs.
        /// </summary>
        public TransitionGroupDocNode GetPrimaryResultsGroup(TransitionGroupDocNode nodeGroup)
        {
            TransitionGroupDocNode nodeGroupPrimary = nodeGroup;
            if (TransitionGroupCount > 1)
            {
                double maxArea = nodeGroup.AveragePeakArea ?? 0;
                int precursorCharge = nodeGroup.TransitionGroup.PrecursorCharge;
                foreach (var nodeGroupChild in TransitionGroups.Where(g =>
                        g.TransitionGroup.PrecursorCharge == precursorCharge &&
                        !ReferenceEquals(g, nodeGroup)))
                {
                    // Only when children match can one precursor provide primary values for another
                    if (!nodeGroup.EquivalentChildren(nodeGroupChild))
                        continue;

                    float peakArea = nodeGroupChild.AveragePeakArea ?? 0;
                    if (peakArea > maxArea)
                    {
                        maxArea = peakArea;
                        nodeGroupPrimary = nodeGroupChild;
                    }
                }
            }
            return nodeGroupPrimary;
        }

        public bool CanTrigger(int? replicateIndex)
        {
            foreach (var nodeGroup in TransitionGroups)
            {
                var nodeGroupPrimary = GetPrimaryResultsGroup(nodeGroup);
                // Return false, if any primary group lacks the ranking information necessary for tMRM/iSRM
                if (!nodeGroupPrimary.HasReplicateRanks(replicateIndex) && !nodeGroupPrimary.HasLibRanks)
                    return false;
            }
            return true;
        }

        public PeptideDocNode Merge(PeptideDocNode nodePepMerge)
        {
            return Merge(nodePepMerge, (n, nMerge) => n.Merge(nMerge));
        }

        public PeptideDocNode Merge(PeptideDocNode nodePepMerge,
            Func<TransitionGroupDocNode, TransitionGroupDocNode, TransitionGroupDocNode> mergeMatch)
        {
            var childrenNew = Children.Cast<TransitionGroupDocNode>().ToList();
            // Remember where all the existing children are
            var dictPepIndex = new Dictionary<TransitionGroup, int>();
            for (int i = 0; i < childrenNew.Count; i++)
            {
                var key = childrenNew[i].TransitionGroup;
                if (!dictPepIndex.ContainsKey(key))
                    dictPepIndex[key] = i;
            }
            // Add the new children to the end, or merge when the node is already present
            foreach (TransitionGroupDocNode nodeGroup in nodePepMerge.Children)
            {
                int i;
                if (!dictPepIndex.TryGetValue(nodeGroup.TransitionGroup, out i))
                    childrenNew.Add(nodeGroup);
                else if (mergeMatch != null)
                    childrenNew[i] = mergeMatch(childrenNew[i], nodeGroup);
            }
            childrenNew.Sort(Peptide.CompareGroups);
            return (PeptideDocNode)ChangeChildrenChecked(childrenNew.Cast<DocNode>().ToArray());
        }

        public PeptideDocNode MergeUserInfo(PeptideDocNode nodePepMerge, SrmSettings settings, SrmSettingsDiff diff)
        {
            var result = Merge(nodePepMerge, (n, nMerge) => n.MergeUserInfo(nodePepMerge, nMerge, settings, diff));
            var annotations = Annotations.Merge(nodePepMerge.Annotations);
            if (!ReferenceEquals(annotations, Annotations))
                result = (PeptideDocNode) result.ChangeAnnotations(annotations);
            return result.UpdateResults(settings);
        }

        public PeptideDocNode ChangeSettings(SrmSettings settingsNew, SrmSettingsDiff diff, bool recurse = true)
        {
            if (diff.Monitor != null)
                diff.Monitor.ProcessMolecule(this);

            // If the peptide has explicit modifications, and the modifications have
            // changed, see if any of the explicit modifications have changed
            var explicitMods = ExplicitMods;
            if (HasExplicitMods &&
                !diff.IsUnexplainedExplicitModificationAllowed &&
                diff.SettingsOld != null &&
                !ReferenceEquals(settingsNew.PeptideSettings.Modifications,
                                 diff.SettingsOld.PeptideSettings.Modifications))
            {
                explicitMods = ExplicitMods.ChangeGlobalMods(settingsNew);
                if (explicitMods == null || !ArrayUtil.ReferencesEqual(explicitMods.GetHeavyModifications().ToArray(),
                                                                       ExplicitMods.GetHeavyModifications().ToArray()))
                {
                    diff = new SrmSettingsDiff(diff, SrmSettingsDiff.ALL);                    
                }
                else if (!ReferenceEquals(explicitMods.StaticModifications, ExplicitMods.StaticModifications))
                {
                    diff = new SrmSettingsDiff(diff, SrmSettingsDiff.PROPS);
                }
            }

            TransitionSettings transitionSettings = settingsNew.TransitionSettings;
            PeptideDocNode nodeResult = this;
            if (!ReferenceEquals(explicitMods, ExplicitMods))
                nodeResult = nodeResult.ChangeExplicitMods(explicitMods);
            nodeResult = nodeResult.UpdateModifiedSequence(settingsNew);

            if (diff.DiffPeptideProps)
            {
                var rt = settingsNew.PeptideSettings.Prediction.RetentionTime;
                bool isStandard = Equals(nodeResult.GlobalStandardType, STANDARD_TYPE_IRT);
                if (rt != null)
                {
                    bool isStandardNew = rt.IsStandardPeptide(nodeResult);
                    if (isStandard ^ isStandardNew)
                        nodeResult = nodeResult.ChangeStandardType(isStandardNew ? STANDARD_TYPE_IRT : null);
                }
                else if (isStandard)
                {
                    nodeResult = nodeResult.ChangeStandardType(null);
                }
            }

            if (diff.DiffTransitionGroups && settingsNew.TransitionSettings.Filter.AutoSelect && AutoManageChildren)
            {
                IList<DocNode> childrenNew = new List<DocNode>();

                PeptideRankId rankId = settingsNew.PeptideSettings.Libraries.RankId;
                bool useHighestRank = (rankId != null && settingsNew.PeptideSettings.Libraries.PeptideCount.HasValue);
                bool isPickedIntensityRank = useHighestRank &&
                                             ReferenceEquals(rankId, LibrarySpec.PEP_RANK_PICKED_INTENSITY);

                Dictionary<Identity, DocNode> mapIdToChild = CreateIdContentToChildMap();
                foreach (TransitionGroup tranGroup in GetTransitionGroups(settingsNew, explicitMods, true))
                {
                    TransitionGroupDocNode nodeGroup;
                    SrmSettingsDiff diffNode = diff;

                    DocNode existing;
                    // Add values that existed before the change, unless using picked intensity ranking,
                    // since this could bias the ranking, otherwise.
                    if (!isPickedIntensityRank && mapIdToChild.TryGetValue(tranGroup, out existing))
                        nodeGroup = (TransitionGroupDocNode)existing;
                    // Add new node
                    else
                    {
                        TransitionDocNode[] transitions = !isPickedIntensityRank
                            ? GetMatchingTransitions(tranGroup, settingsNew, explicitMods)
                            : null;

                        nodeGroup = new TransitionGroupDocNode(tranGroup, transitions);
                        // If not recursing, then ChangeSettings will not be called on nodeGroup.  So, make
                        // sure its precursor m/z is set correctly.
                        if (!recurse)
                            nodeGroup = nodeGroup.ChangePrecursorMz(settingsNew, explicitMods);
                        diffNode = SrmSettingsDiff.ALL;
                    }

                    if (nodeGroup != null)
                    {
                        TransitionGroupDocNode nodeChanged = recurse
                            ? nodeGroup.ChangeSettings(settingsNew, nodeResult, explicitMods, diffNode)
                            : nodeGroup;
                        if (transitionSettings.IsMeasurablePrecursor(nodeChanged.PrecursorMz))
                            childrenNew.Add(nodeChanged);
                    }
                }

                // If only using rank limited peptides, then choose only the single
                // highest ranked precursor charge.
                if (useHighestRank)
                {
                    childrenNew = FilterHighestRank(childrenNew, rankId);

                    // If using picked intensity, make sure original nodes are replaced
                    if (isPickedIntensityRank)
                    {
                        for (int i = 0; i < childrenNew.Count; i++)
                        {
                            var nodeNew = (TransitionGroupDocNode) childrenNew[i];
                            DocNode existing;
                            if (mapIdToChild.TryGetValue(nodeNew.TransitionGroup, out existing))
                                childrenNew[i] = existing;
                        }
                    }
                }

                nodeResult = (PeptideDocNode) nodeResult.ChangeChildrenChecked(childrenNew);                
            }
            else
            {
                // Even with auto-select off, transition groups for which there is
                // no longer a precursor calculator must be removed.
                if (diff.DiffTransitionGroups && nodeResult.HasHeavyTransitionGroups)
                {
                    IList<DocNode> childrenNew = new List<DocNode>();
                    foreach (TransitionGroupDocNode nodeGroup in nodeResult.Children)
                    {
                        if (settingsNew.HasPrecursorCalc(nodeGroup.TransitionGroup.LabelType, explicitMods))
                            childrenNew.Add(nodeGroup);
                    }

                    nodeResult = (PeptideDocNode)nodeResult.ChangeChildrenChecked(childrenNew);
                }

                // Update properties and children, if necessary
                if (diff.DiffTransitionGroupProps ||
                    diff.DiffTransitions || diff.DiffTransitionProps ||
                    diff.DiffResults)
                {
                    IList<DocNode> childrenNew = new List<DocNode>();

                    // Enumerate the nodes making necessary changes.
                    foreach (TransitionGroupDocNode nodeGroup in nodeResult.Children)
                    {
                        TransitionGroupDocNode nodeChanged = nodeGroup.ChangeSettings(settingsNew, nodeResult, explicitMods, diff);
                        // Skip if the node can no longer be measured on the target instrument
                        if (!transitionSettings.IsMeasurablePrecursor(nodeChanged.PrecursorMz))
                            continue;
                        // Skip this node, if it is heavy and the update caused it to have the
                        // same m/z value as the light value.
                        if (!nodeChanged.TransitionGroup.LabelType.IsLight &&
                            !Peptide.IsCustomIon) // No mods on customs
                        {
                            double precursorMassLight = settingsNew.GetPrecursorMass(
                                IsotopeLabelType.light, Peptide.Sequence, explicitMods);
                            double precursorMzLight = SequenceMassCalc.GetMZ(precursorMassLight,
                                                                             nodeChanged.TransitionGroup.PrecursorCharge);
                            if (nodeChanged.PrecursorMz == precursorMzLight)
                                continue;
                        }

                        childrenNew.Add(nodeChanged);
                    }

                    nodeResult = (PeptideDocNode)nodeResult.ChangeChildrenChecked(childrenNew);
                }                
            }

            if (diff.DiffResults || ChangedResults(nodeResult))
                nodeResult = nodeResult.UpdateResults(settingsNew /*, diff*/);

            return nodeResult;
        }

        public IEnumerable<TransitionGroup> GetTransitionGroups(SrmSettings settings, ExplicitMods explicitMods, bool useFilter)
        {
            return Peptide.GetTransitionGroups(settings, this, explicitMods, useFilter);
        }

        public TransitionGroup[] GetNonProteomicChildren()
        {
            return
                TransitionGroups.Where(tranGroup => tranGroup.TransitionGroup.IsCustomIon)
                    .Select(groupNode => groupNode.TransitionGroup)
                    .ToArray();
        }

        /// <summary>
        /// Make sure children are preserved as much as possible.  It may not be possible
        /// to always preserve children, because the settings in the target document may
        /// not allow certain states (e.g. label types that to not exist in the target).
        /// </summary>
        public PeptideDocNode EnsureChildren(SrmSettings settings, bool peptideList)
        {
            var result = this;

            // Make a first attempt at changing to the new settings to figure out
            // whether this will change the children.
            var changed = result.ChangeSettings(settings, SrmSettingsDiff.ALL);
            // If the children are auto-managed, and they changed, figure out whether turning off
            // auto-manage will allow the children to be preserved better.
            if (result.AutoManageChildren && !AreEquivalentChildren(result.Children, changed.Children))
            {
                // Turn off auto-manage and change again.
                var resultAutoManageFalse = (PeptideDocNode)result.ChangeAutoManageChildren(false);
                var changedAutoManageFalse = resultAutoManageFalse.ChangeSettings(settings, SrmSettingsDiff.ALL);
                // If the children are not the same as they were with auto-manage on, then use
                // a the version of this node with auto-manage turned off.
                if (!AreEquivalentChildren(changed.Children, changedAutoManageFalse.Children))
                {
                    result = resultAutoManageFalse;
                    changed = changedAutoManageFalse;
                }
            }
            // If this is being added to a peptide list, but was a FASTA sequence,
            // make sure the Peptide ID no longer points to the old FASTA sequence.
            if (peptideList && Peptide.FastaSequence != null)
            {
                result = new PeptideDocNode(new Peptide(null, Peptide.Sequence, null, null, Peptide.MissedCleavages), settings,
                                            result.ExplicitMods, result.SourceKey, result.ExplicitRetentionTime, new TransitionGroupDocNode[0], result.AutoManageChildren); 
            }
            // Create a new child list, using existing children where GlobalIndexes match.
            var dictIndexToChild = Children.ToDictionary(child => child.Id.GlobalIndex);
            var listChildren = new List<DocNode>();
            foreach (TransitionGroupDocNode nodePep in changed.Children)
            {
                DocNode child;
                if (dictIndexToChild.TryGetValue(nodePep.Id.GlobalIndex, out child))
                {
                    listChildren.Add(((TransitionGroupDocNode)child).EnsureChildren(result, ExplicitMods, settings));
                }
            }
            return (PeptideDocNode)result.ChangeChildrenChecked(listChildren);
        }

        public static bool AreEquivalentChildren(IList<DocNode> children1, IList<DocNode> children2)
        {
            if(children1.Count != children2.Count)
                return false;
            for (int i = 0; i < children1.Count; i++)
            {
                if(!Equals(children1[i].Id, children2[i].Id))
                    return false;
            }
            return true;
        }

        public PeptideDocNode EnsureMods(PeptideModifications source, PeptideModifications target,
                                         MappedList<string, StaticMod> defSetStat, MappedList<string, StaticMod> defSetHeavy)
        {
            // Create explicit mods matching the implicit mods on this peptide for each document.
            var sourceImplicitMods = new ExplicitMods(this, source.StaticModifications, defSetStat, source.GetHeavyModifications(), defSetHeavy);
            var targetImplicitMods = new ExplicitMods(this, target.StaticModifications, defSetStat, target.GetHeavyModifications(), defSetHeavy);
            
            // If modifications match, no need to create explicit modifications for the peptide.
            if (sourceImplicitMods.Equals(targetImplicitMods))
                return this;

            // Add explicit mods if static mods not implicit in the target document.
            IList<ExplicitMod> newExplicitStaticMods = null;
            bool preserveVariable = HasVariableMods;
            // Preserve non-variable explicit modifications
            if (!preserveVariable && HasExplicitMods && ExplicitMods.StaticModifications != null)
            {
                // If they are not the same as the implicit modifications in the new document
                if (!ArrayUtil.EqualsDeep(ExplicitMods.StaticModifications, targetImplicitMods.StaticModifications))
                    newExplicitStaticMods = ExplicitMods.StaticModifications;
            }
            else if (!ArrayUtil.EqualsDeep(sourceImplicitMods.StaticModifications, targetImplicitMods.StaticModifications))
            {
                preserveVariable = false;
                newExplicitStaticMods = sourceImplicitMods.StaticModifications;
            }
            else if (preserveVariable)
            {
                newExplicitStaticMods = ExplicitMods.StaticModifications;
            }
                
            // Drop explicit mods if matching implicit mods are found in the target document.
            IList<TypedExplicitModifications> newExplicitHeavyMods = new List<TypedExplicitModifications>();
            // For each heavy label type, add explicit mods if static mods not found in the target document.
            var newTypedStaticMods = newExplicitStaticMods != null
                ? new TypedExplicitModifications(Peptide, IsotopeLabelType.light, newExplicitStaticMods)
                : null;
            foreach (TypedExplicitModifications targetDocMod in targetImplicitMods.GetHeavyModifications())
            {
                // Use explicit modifications when available.  Otherwise, compare against new implicit modifications
                IList<ExplicitMod> heavyMods = (HasExplicitMods ? ExplicitMods.GetModifications(targetDocMod.LabelType) : null) ??
                    sourceImplicitMods.GetModifications(targetDocMod.LabelType);
                if (heavyMods != null && !ArrayUtil.EqualsDeep(heavyMods, targetDocMod.Modifications) && heavyMods.Count > 0)
                {
                    var newTypedHeavyMods = new TypedExplicitModifications(Peptide, targetDocMod.LabelType, heavyMods);
                    newTypedHeavyMods = newTypedHeavyMods.AddModMasses(newTypedStaticMods);
                    newExplicitHeavyMods.Add(newTypedHeavyMods);
                }
            }

            if (newExplicitStaticMods != null || newExplicitHeavyMods.Count > 0)
                return ChangeExplicitMods(new ExplicitMods(Peptide, newExplicitStaticMods, newExplicitHeavyMods, preserveVariable));
            return ChangeExplicitMods(null);
        }

        private static IList<DocNode> FilterHighestRank(IList<DocNode> childrenNew, PeptideRankId rankId)
        {
            if (childrenNew.Count < 2)
                return childrenNew;
            int maxCharge = 0;
            float maxValue = float.MinValue;
            foreach (TransitionGroupDocNode nodeGroup in childrenNew)
            {
                float rankValue = nodeGroup.GetRankValue(rankId);
                if (rankValue > maxValue)
                {
                    maxCharge = nodeGroup.TransitionGroup.PrecursorCharge;
                    maxValue = rankValue;
                }
            }
            var listHighestRankChildren = new List<DocNode>();
            foreach (TransitionGroupDocNode nodeGroup in childrenNew)
            {
                if (nodeGroup.TransitionGroup.PrecursorCharge == maxCharge)
                    listHighestRankChildren.Add(nodeGroup);
            }
            return listHighestRankChildren;
        }

        public TransitionDocNode[] GetMatchingTransitions(TransitionGroup tranGroup, SrmSettings settings, ExplicitMods explicitMods)
        {
            int iMatch = Children.IndexOf(nodeGroup =>
                                          ((TransitionGroupDocNode)nodeGroup).TransitionGroup.PrecursorCharge == tranGroup.PrecursorCharge);
            if (iMatch == -1)
                return null;
            TransitionGroupDocNode nodeGroupMatching = (TransitionGroupDocNode) Children[iMatch];
            // If the matching node is auto-managed, and auto-select is on in the settings,
            // then returning no transitions should allow transitions to be chosen correctly
            // automatically.
            if (nodeGroupMatching.AutoManageChildren && settings.TransitionSettings.Filter.AutoSelect &&
                // Having disconnected libraries can mess up automatic picking
                    settings.PeptideSettings.Libraries.DisconnectedLibraries == null)
                return null;

            return tranGroup.GetMatchingTransitions(settings, nodeGroupMatching, explicitMods);
        }

        private PeptideDocNode UpdateResults(SrmSettings settingsNew /*, SrmSettingsDiff diff*/)
        {
            // First check whether any child results are present
            if (!settingsNew.HasResults || Children.Count == 0)
            {
                if (!HasResults)
                    return this;
                return ChangeResults(null);
            }

            // Update the results summary
            var resultsCalc = new PeptideResultsCalculator(settingsNew);
            foreach (TransitionGroupDocNode nodeGroup in Children)
                resultsCalc.AddGroupChromInfo(nodeGroup);

            return resultsCalc.UpdateResults(this);
        }

        private bool ChangedResults(DocNodeParent nodePeptide)
        {
            if (nodePeptide.Children.Count != Children.Count)
                return true;

            int iChild = 0;
            foreach (TransitionGroupDocNode nodeGroup in Children)
            {
                // Results will differ if the identies of the children differ
                // at all.
                var nodeGroup2 = (TransitionGroupDocNode)nodePeptide.Children[iChild];
                if (!ReferenceEquals(nodeGroup.Id, nodeGroup2.Id))
                    return true;

                // or if the results for any child have changed
                if (!ReferenceEquals(nodeGroup.Results, nodeGroup2.Results))
                    return true;

                iChild++;
            }
            return false;
        }

        public override string GetDisplayText(DisplaySettings settings)
        {
            return PeptideTreeNode.DisplayText(this, settings);
        }

        private sealed class PeptideResultsCalculator
        {
            private readonly List<PeptideChromInfoListCalculator> _listResultCalcs =
                new List<PeptideChromInfoListCalculator>();

            public PeptideResultsCalculator(SrmSettings settings)
            {
                Settings = settings;
            }

            private SrmSettings Settings { get; set; }
            private int TransitionGroupCount { get; set; }

            public void AddGroupChromInfo(TransitionGroupDocNode nodeGroup)
            {
                TransitionGroupCount++;

                if (nodeGroup.HasResults)
                {
                    int countResults = nodeGroup.Results.Count;
                    while (_listResultCalcs.Count < countResults)
                    {
                        var calc = new PeptideChromInfoListCalculator(Settings, _listResultCalcs.Count);
                        _listResultCalcs.Add(calc);
                    }
                    for (int i = 0; i < countResults; i++)
                    {
                        var calc = _listResultCalcs[i];
                        calc.AddChromInfoList(nodeGroup);
                        foreach (TransitionDocNode nodeTran in nodeGroup.Children)
                            calc.AddChromInfoList(nodeTran);
                    }
                }
            }

            public PeptideDocNode UpdateResults(PeptideDocNode nodePeptide)
            {
                var listChromInfoList = _listResultCalcs.ConvertAll(calc =>
                                                                    calc.CalcChromInfoList(TransitionGroupCount));
                var results = Results<PeptideChromInfo>.Merge(nodePeptide.Results, listChromInfoList);
                if (!ReferenceEquals(results, nodePeptide.Results))
                    nodePeptide = nodePeptide.ChangeResults(results);

                var listGroupsNew = new List<DocNode>();
                foreach (TransitionGroupDocNode nodeGroup in nodePeptide.Children)
                {
                    // Update transition group ratios
                    var nodeGroupConvert = nodeGroup;
                    bool isMatching = nodeGroup.RelativeRT == RelativeRT.Matching;
                    var listGroupInfoList = _listResultCalcs.ConvertAll(
                        calc => calc.UpdateTransitonGroupRatios(nodeGroupConvert,
                                                                nodeGroupConvert.HasResults
                                                                    ? nodeGroupConvert.Results[calc.ResultsIndex]
                                                                    : null,
                                                                isMatching));
                    var resultsGroup = Results<TransitionGroupChromInfo>.Merge(nodeGroup.Results, listGroupInfoList);
                    var nodeGroupNew = nodeGroup;
                    if (!ReferenceEquals(resultsGroup, nodeGroup.Results))
                        nodeGroupNew = nodeGroup.ChangeResults(resultsGroup);

                    var listTransNew = new List<DocNode>();
                    foreach (TransitionDocNode nodeTran in nodeGroup.Children)
                    {
                        // Update transition ratios
                        var nodeTranConvert = nodeTran;
                        var listTranInfoList = _listResultCalcs.ConvertAll(
                            calc => calc.UpdateTransitionRatios(nodeTranConvert,
                                                               nodeTranConvert.Results[calc.ResultsIndex],
                                                               isMatching));
                        var resultsTran = Results<TransitionChromInfo>.Merge(nodeTran.Results, listTranInfoList);
                        listTransNew.Add(ReferenceEquals(resultsTran, nodeTran.Results)
                                             ? nodeTran
                                             : nodeTran.ChangeResults(resultsTran));
                    }
                    listGroupsNew.Add(nodeGroupNew.ChangeChildrenChecked(listTransNew));
                }
                return (PeptideDocNode) nodePeptide.ChangeChildrenChecked(listGroupsNew);
            }
        }

        private sealed class PeptideChromInfoListCalculator
        {
            public PeptideChromInfoListCalculator(SrmSettings settings, int resultsIndex)
            {
                ResultsIndex = resultsIndex;
                Settings = settings;
                Calculators = new Dictionary<int, PeptideChromInfoCalculator>();
            }

            public int ResultsIndex { get; private set; }

            private SrmSettings Settings { get; set; }
            private Dictionary<int, PeptideChromInfoCalculator> Calculators { get; set; }

            public void AddChromInfoList(TransitionGroupDocNode nodeGroup)
            {
                var listInfo = nodeGroup.Results[ResultsIndex];
                if (listInfo == null)
                    return;

                foreach (var info in listInfo)
                {
                    if (info.OptimizationStep != 0)
                        continue;

                    PeptideChromInfoCalculator calc;
                    if (!Calculators.TryGetValue(info.FileIndex, out calc))
                    {
                        calc = new PeptideChromInfoCalculator(Settings, ResultsIndex);
                        Calculators.Add(info.FileIndex, calc);
                    }
                    calc.AddChromInfo(nodeGroup, info);
                }
            }

            public void AddChromInfoList(TransitionDocNode nodeTran)
            {
                var listInfo = nodeTran.Results[ResultsIndex];
                if (listInfo == null)
                    return;

                foreach (var info in listInfo)
                {
                    if (info.OptimizationStep != 0)
                        continue;

                    PeptideChromInfoCalculator calc;
                    if (!Calculators.TryGetValue(info.FileIndex, out calc))
                    {
                        calc = new PeptideChromInfoCalculator(Settings, ResultsIndex);
                        Calculators.Add(info.FileIndex, calc);
                    }
                    calc.AddChromInfo(nodeTran, info);
                }
            }

            public IList<PeptideChromInfo> CalcChromInfoList(int transitionGroupCount)
            {
                if (Calculators.Count == 0)
                    return null;

                var listCalc = new List<PeptideChromInfoCalculator>(Calculators.Values);
                listCalc.Sort((c1, c2) => c1.FileOrder - c2.FileOrder);

                var listInfo = listCalc.ConvertAll(calc => calc.CalcChromInfo(transitionGroupCount));
                return (listInfo[0] != null ? listInfo : null);
            }

            public IList<TransitionChromInfo> UpdateTransitionRatios(TransitionDocNode nodeTran,
                                                                    IList<TransitionChromInfo> listInfo,
                                                                    bool isMatching)
            {
                if (Calculators.Count == 0 || listInfo == null)
                    return null;

                var listInfoNew = new List<TransitionChromInfo>();
                var standardTypes = Settings.PeptideSettings.Modifications.RatioInternalStandardTypes;
                foreach (var info in listInfo)
                {
                    PeptideChromInfoCalculator calc;
                    if (!Calculators.TryGetValue(info.FileIndex, out calc))
                        Assume.Fail();    // Should never happen
                    else
                    {
                        var infoNew = info;
                        var labelType = nodeTran.Transition.Group.LabelType;

                        int count = standardTypes.Count;
                        if (calc.HasGlobalArea)
                            count++;
                        var ratios = new float?[count];
                        for (int i = 0; i < standardTypes.Count; i++)
                            ratios[i] = calc.CalcTransitionRatio(nodeTran, labelType, standardTypes[i]);
                        if (calc.HasGlobalArea)
                            ratios[count - 1] = calc.CalcTransitionGlobalRatio(nodeTran, labelType);
                        if (!ArrayUtil.EqualsDeep(ratios, info.Ratios))
                            infoNew = infoNew.ChangeRatios(ratios);

                        if (isMatching && calc.IsSetMatching && !infoNew.IsUserSetMatched)
                            infoNew = infoNew.ChangeUserSet(UserSet.MATCHED);

                        listInfoNew.Add(infoNew);
                    }
                }
                if (ArrayUtil.ReferencesEqual(listInfo, listInfoNew))
                    return listInfo;
                return listInfoNew;
            }

            public IList<TransitionGroupChromInfo> UpdateTransitonGroupRatios(TransitionGroupDocNode nodeGroup,
                                                                              IList<TransitionGroupChromInfo> listInfo,
                                                                              bool isMatching)
            {
                if (Calculators.Count == 0 || listInfo == null)
                    return null;

                var listInfoNew = new List<TransitionGroupChromInfo>();
                var standardTypes = Settings.PeptideSettings.Modifications.RatioInternalStandardTypes;
                foreach (var info in listInfo)
                {
                    PeptideChromInfoCalculator calc;
                    if (!Calculators.TryGetValue(info.FileIndex, out calc))
                        Assume.Fail();    // Should never happen
                    else
                    {
                        var infoNew = info;
                        var labelType = nodeGroup.TransitionGroup.LabelType;

                        int count = standardTypes.Count;
                        if (calc.HasGlobalArea)
                            count++;
                        var ratios = new RatioValue[count];
                        for (int i = 0; i < standardTypes.Count; i++)
                        {
                            ratios[i] = calc.CalcTransitionGroupRatio(nodeGroup, labelType, standardTypes[i]);
                        }
                        if (calc.HasGlobalArea)
                            ratios[count - 1] = calc.CalcTransitionGroupGlobalRatio(nodeGroup, labelType);
                        if (!ArrayUtil.EqualsDeep(ratios, info.Ratios))
                            infoNew = infoNew.ChangeRatios(ratios);

                        if (isMatching && calc.IsSetMatching && !infoNew.IsUserSetMatched)
                            infoNew = infoNew.ChangeUserSet(UserSet.MATCHED);

                        listInfoNew.Add(infoNew);
                    }
                }
                if (ArrayUtil.ReferencesEqual(listInfo, listInfoNew))
                    return listInfo;
                return listInfoNew;
            }
        }

        private sealed class PeptideChromInfoCalculator
        {
            public PeptideChromInfoCalculator(SrmSettings settings, int resultsIndex)
            {
                Settings = settings;
                ResultsIndex = resultsIndex;
                TranAreas = new Dictionary<TransitionKey, float>();
            }

            private SrmSettings Settings { get; set; }
            private int ResultsIndex { get; set; }
            private ChromFileInfoId FileId { get; set; }
            public int FileOrder { get; private set; }
            private double PeakCountRatioTotal { get; set; }
            private int ResultsCount { get; set; }
            private int RetentionTimesMeasured { get; set; }
            private double RetentionTimeTotal { get; set; }
            private double GlobalStandardArea { get; set; }

            private Dictionary<TransitionKey, float> TranAreas { get; set; }

            public bool HasGlobalArea { get { return GlobalStandardArea > 0; }}
            public bool IsSetMatching { get; private set; }

// ReSharper disable UnusedParameter.Local
            public void AddChromInfo(TransitionGroupDocNode nodeGroup,
                                     TransitionGroupChromInfo info)
// ReSharper restore UnusedParameter.Local
            {
                if (info == null)
                    return;

                Assume.IsTrue(FileId == null || ReferenceEquals(info.FileId, FileId));
                var fileIdPrevious = FileId;
                FileId = info.FileId;
                FileOrder = Settings.MeasuredResults.Chromatograms[ResultsIndex].IndexOfId(FileId);

                // First time through calculate the global standard area for this file
                if (fileIdPrevious == null)
                    GlobalStandardArea = Settings.CalcGlobalStandardArea(ResultsIndex, FileId);

                ResultsCount++;
                PeakCountRatioTotal += info.PeakCountRatio;
                if (info.RetentionTime.HasValue)
                {
                    RetentionTimesMeasured++;
                    RetentionTimeTotal += info.RetentionTime.Value;
                }

                if (info.UserSet == UserSet.MATCHED)
                    IsSetMatching = true;
            }

            public void AddChromInfo(TransitionDocNode nodeTran, TransitionChromInfo info)
            {
                // Only add non-zero areas
                if (info.Area == 0)
                    return;

                var key = new TransitionKey(nodeTran.Key);
                if (TranAreas.ContainsKey(key))
                    throw new InvalidDataException(string.Format(Resources.PeptideChromInfoCalculator_AddChromInfo_Duplicate_transition___0___found_for_peak_areas, nodeTran.Transition));
                TranAreas.Add(key, info.Area);
            }

            public PeptideChromInfo CalcChromInfo(int transitionGroupCount)
            {
                if (ResultsCount == 0)
                    return null;

                float peakCountRatio = (float) (PeakCountRatioTotal/transitionGroupCount);

                float? retentionTime = null;
                if (RetentionTimesMeasured > 0)
                    retentionTime = (float) (RetentionTimeTotal/RetentionTimesMeasured);
                var mods = Settings.PeptideSettings.Modifications;
                var listRatios = new List<PeptideLabelRatio>();
                // First add ratios to reference peptides
                foreach (var standardType in mods.RatioInternalStandardTypes)
                {
                    foreach (var labelType in mods.GetModificationTypes())
                    {
                        if (ReferenceEquals(standardType, labelType))
                            continue;

                        RatioValue ratio = CalcTransitionGroupRatio(-1, labelType, standardType);
                        listRatios.Add(new PeptideLabelRatio(labelType, standardType, ratio));
                    }                    
                }
                // Then add ratios to global standards
                foreach (var labelType in mods.GetModificationTypes())
                {
                    RatioValue ratio = CalcTransitionGroupGlobalRatio(-1, labelType);
                    listRatios.Add(new PeptideLabelRatio(labelType, null, ratio));
                }

                return new PeptideChromInfo(FileId, peakCountRatio, retentionTime, listRatios.ToArray());
            }

            public float? CalcTransitionGlobalRatio(TransitionDocNode nodeTran,
                                                    IsotopeLabelType labelType)
            {
                if (GlobalStandardArea == 0)
                    return null;

                float areaNum;
                var keyNum = new TransitionKey(nodeTran.Key, labelType);
                if (!TranAreas.TryGetValue(keyNum, out areaNum))
                    return null;
                return (float) (areaNum / GlobalStandardArea);
            }

            public float? CalcTransitionRatio(TransitionDocNode nodeTran,
                                              IsotopeLabelType labelTypeNum, IsotopeLabelType labelTypeDenom)
            {
                // Avoid 1.0 ratios for self-to-self
                if (ReferenceEquals(labelTypeNum, labelTypeDenom))
                    return null;

                float areaNum, areaDenom;
                var keyNum = new TransitionKey(nodeTran.Key, labelTypeNum);
                var keyDenom = new TransitionKey(nodeTran.Key, labelTypeDenom);
                if (!TranAreas.TryGetValue(keyNum, out areaNum) ||
                    !TranAreas.TryGetValue(keyDenom, out areaDenom))
                    return null;
                return areaNum/areaDenom;
            }

            public RatioValue CalcTransitionGroupGlobalRatio(TransitionGroupDocNode nodeGroup,
                                                             IsotopeLabelType labelTypeNum)
            {
                return CalcTransitionGroupGlobalRatio(nodeGroup.TransitionGroup.PrecursorCharge,
                                                      labelTypeNum);
            }

            private RatioValue CalcTransitionGroupGlobalRatio(int precursorCharge, IsotopeLabelType labelType)
            {
                if (GlobalStandardArea == 0)
                    return null;

                double num = 0;
                foreach (var pair in GetAreaPairs(labelType))
                {
                    var key = pair.Key;
                    if (precursorCharge != -1 && key.PrecursorCharge != precursorCharge)
                        continue;
                    num += pair.Value;
                }

                return new RatioValue(num / GlobalStandardArea);
            }

            public RatioValue CalcTransitionGroupRatio(TransitionGroupDocNode nodeGroup,
                                                       IsotopeLabelType labelTypeNum,
                                                       IsotopeLabelType labelTypeDenom)
            {
                return CalcTransitionGroupRatio(nodeGroup.TransitionGroup.PrecursorCharge,
                                                labelTypeNum, labelTypeDenom);
            }

            private RatioValue CalcTransitionGroupRatio(int precursorCharge,
                                                        IsotopeLabelType labelTypeNum,
                                                        IsotopeLabelType labelTypeDenom)
            {
                // Avoid 1.0 ratios for self-to-self
                if (ReferenceEquals(labelTypeNum, labelTypeDenom))
                {
                    return null;
                }

                List<double> numerators = new List<double>();
                List<double> denominators = new List<double>();

                foreach (var pair in GetAreaPairs(labelTypeNum))
                {
                    var key = pair.Key;
                    if (precursorCharge != -1 && key.PrecursorCharge != precursorCharge)
                        continue;

                    float areaNum = pair.Value;
                    float areaDenom;
                    if (!TranAreas.TryGetValue(new TransitionKey(key, labelTypeDenom), out areaDenom))
                        continue;

                    numerators.Add(areaNum);
                    denominators.Add(areaDenom);
                }

                return RatioValue.Calculate(numerators, denominators);
            }

            private IEnumerable<KeyValuePair<TransitionKey, float>> GetAreaPairs(IsotopeLabelType labelType)
            {
                return from pair in TranAreas
                       where ReferenceEquals(labelType, pair.Key.LabelType)
                       select pair;
            }
        }

        private struct TransitionKey
        {
            private readonly IonType _ionType;
            private readonly CustomIon _ion;
            private readonly int _ionOrdinal;
            private readonly int _massIndex;
            private readonly int? _decoyMassShift;
            private readonly int _charge;
            private readonly int _precursorCharge;
            private readonly TransitionLosses _losses;
            private readonly IsotopeLabelType _labelType;

            public TransitionKey(TransitionLossKey tranLossKey)
                : this (tranLossKey, tranLossKey.Transition.Group.LabelType)
            {
            }

            public TransitionKey(TransitionLossKey tranLossKey, IsotopeLabelType labelType)
            {
                var transition = tranLossKey.Transition;
                _ionType = transition.IonType;
                _ion = transition.CustomIon;
                _ionOrdinal = transition.Ordinal;
                _massIndex = transition.MassIndex;
                _decoyMassShift = transition.DecoyMassShift;
                _charge = transition.Charge;
                _precursorCharge = transition.Group.PrecursorCharge;
                _losses = tranLossKey.Losses;
                _labelType = labelType;
            }

            public TransitionKey(TransitionKey key, IsotopeLabelType labelType)
            {
                _ionType = key._ionType;
                _ion = key._ion;
                _ionOrdinal = key._ionOrdinal;
                _massIndex = key._massIndex;
                _decoyMassShift = key._decoyMassShift;
                _charge = key._charge;
                _precursorCharge = key._precursorCharge;
                _losses = key._losses;
                _labelType = labelType;
            }

            public int PrecursorCharge { get { return _precursorCharge; } }
            public IsotopeLabelType LabelType { get { return _labelType; } }

            #region object overrides

            private bool Equals(TransitionKey other)
            {
                return Equals(other._ionType, _ionType) &&
                       Equals(_ion, other._ion) &&
                       other._ionOrdinal == _ionOrdinal &&
                       other._massIndex == _massIndex &&
                       Equals(other._decoyMassShift, _decoyMassShift) &&
                       other._charge == _charge &&
                       other._precursorCharge == _precursorCharge &&
                       Equals(other._losses, _losses) &&
                       Equals(other._labelType, _labelType);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (obj.GetType() != typeof (TransitionKey)) return false;
                return Equals((TransitionKey) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int result = _ionType.GetHashCode();
                    result = (result*397) ^ (_ion == null ? 0 : _ion.GetHashCode());
                    result = (result*397) ^ _ionOrdinal;
                    result = (result*397) ^ _massIndex;
                    result = (result*397) ^ (_decoyMassShift.HasValue ? _decoyMassShift.Value : 0);
                    result = (result*397) ^ _charge;
                    result = (result*397) ^ _precursorCharge;
                    result = (result*397) ^ (_losses != null ? _losses.GetHashCode() : 0);
                    result = (result*397) ^ _labelType.GetHashCode();
                    return result;
                }
            }

            #endregion
        }

        #region object overrides

        public bool Equals(PeptideDocNode other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) &&
                Equals(other.ExplicitMods, ExplicitMods) &&
                Equals(other.SourceKey, SourceKey) &&
                other.Rank.Equals(Rank) &&
                Equals(other.Results, Results) &&
                Equals(other.ExplicitRetentionTime, ExplicitRetentionTime) &&
                other.BestResult == BestResult;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as PeptideDocNode);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result*397) ^ (ExplicitMods != null ? ExplicitMods.GetHashCode() : 0);
                result = (result*397) ^ (SourceKey != null ? SourceKey.GetHashCode() : 0);
                result = (result*397) ^ (Rank.HasValue ? Rank.Value : 0);
                result = (result*397) ^ (ExplicitRetentionTime != null ? ExplicitRetentionTime.GetHashCode() : 0);
                result = (result*397) ^ (Results != null ? Results.GetHashCode() : 0);
                result = (result*397) ^ BestResult;
                return result;
            }
        }

        public override string ToString()
        {
            return Rank.HasValue
                       ? string.Format(Resources.PeptideDocNodeToString__0__rank__1__, Peptide, Rank)
                       : Peptide.ToString();
        }

        #endregion
    }

    public struct PeptidePrecursorPair
    {
        public PeptidePrecursorPair(PeptideDocNode nodePep, TransitionGroupDocNode nodeGroup) : this()
        {
            NodePep = nodePep;
            NodeGroup = nodeGroup;
        }

        public PeptideDocNode NodePep { get; private set; }
        public TransitionGroupDocNode NodeGroup { get; private set; }
    }

    public class ExplicitRetentionTimeInfo
    {
        public ExplicitRetentionTimeInfo(double retentionTime, double? retentionTimeWindow)
        {
            RetentionTime = retentionTime;
            RetentionTimeWindow = retentionTimeWindow;
        }

        public double RetentionTime { get; private set; }
        public double? RetentionTimeWindow { get; private set; }

        public bool Equals(ExplicitRetentionTimeInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.RetentionTime, RetentionTime) &&
                Equals(other.RetentionTimeWindow, RetentionTimeWindow);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as ExplicitRetentionTimeInfo);
        }


        public override int GetHashCode()
        {
            int result = RetentionTime.GetHashCode() ;
            if (RetentionTimeWindow.HasValue)
                result = (result*397) ^ RetentionTimeWindow.Value.GetHashCode() ;
            return result;
        }

    }
}