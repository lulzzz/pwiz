﻿/*
 * Original author: Brian PRatt <bspratt .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2019 University of Washington - Seattle, WA
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

namespace pwiz.Skyline.Util
{
    /// <summary>
    /// For dialogs which never require any "peptide"=>"molecule" translation
    /// </summary>
    public class ModeUIInvariantFormEx : FormEx
    {
        public ModeUIInvariantFormEx()
        {
            GetModeUIHelper().IgnoreModeUI = true; // Don't want any "peptide"=>"molecule" translation
        }
    }
}