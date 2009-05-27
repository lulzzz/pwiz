//
// ChromatogramList_ABI.cpp
//
//
// Original author: Matt Chambers <matt.chambers .@. vanderbilt.edu>
//
// Copyright 2009 Vanderbilt University - Nashville, TN 37232
//
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
//


#define PWIZ_SOURCE

#ifdef PWIZ_READER_ABI

#include "ChromatogramList_ABI.hpp"
#include "Reader_ABI_Detail.hpp"
#include "pwiz/utility/misc/SHA1Calculator.hpp"
#include "boost/shared_ptr.hpp"
#include "pwiz/utility/misc/String.hpp"
#include "pwiz/utility/misc/Filesystem.hpp"
#include <boost/bind.hpp>
#include <iostream>

using namespace std;
using boost::shared_ptr;
using boost::lexical_cast;
using boost::bad_lexical_cast;


namespace pwiz {
namespace msdata {
namespace detail {


PWIZ_API_DECL ChromatogramList_ABI::ChromatogramList_ABI(const MSData& msd, WiffFilePtr wifffile, int sample)
:   msd_(msd),
    wifffile_(wifffile),
    sample(sample),
    size_(0),
    indexInitialized_(BOOST_ONCE_INIT)
{
}


PWIZ_API_DECL ChromatogramList_ABI::~ChromatogramList_ABI()
{
}


PWIZ_API_DECL size_t ChromatogramList_ABI::size() const
{
    boost::call_once(indexInitialized_, boost::bind(&ChromatogramList_ABI::createIndex, this));
    return size_;
}


PWIZ_API_DECL const ChromatogramIdentity& ChromatogramList_ABI::chromatogramIdentity(size_t index) const
{
    boost::call_once(indexInitialized_, boost::bind(&ChromatogramList_ABI::createIndex, this));
    if (index>size_)
        throw runtime_error(("[ChromatogramList_ABI::chromatogramIdentity()] Bad index: " 
                            + lexical_cast<string>(index)).c_str());
    return index_[index];
}


PWIZ_API_DECL size_t ChromatogramList_ABI::find(const string& id) const
{
    boost::call_once(indexInitialized_, boost::bind(&ChromatogramList_ABI::createIndex, this));

    map<string, size_t>::const_iterator scanItr = idToIndexMap_.find(id);
    if (scanItr == idToIndexMap_.end())
        return size_;
    return scanItr->second;
}


PWIZ_API_DECL ChromatogramPtr ChromatogramList_ABI::chromatogram(size_t index, bool getBinaryData) const
{
    boost::call_once(indexInitialized_, boost::bind(&ChromatogramList_ABI::createIndex, this));
    if (index>size_)
        throw runtime_error(("[ChromatogramList_ABI::chromatogram()] Bad index: " 
                            + lexical_cast<string>(index)).c_str());

    
    // allocate a new Chromatogram
    IndexEntry& ie = index_[index];
    ChromatogramPtr result = ChromatogramPtr(new Chromatogram);
    if (!result.get())
        throw std::runtime_error("[ChromatogramList_Thermo::chromatogram()] Allocation error.");

    result->index = index;
    result->id = ie.id;
    result->set(ie.chromatogramType);

    switch (ie.chromatogramType)
    {
        case MS_TIC_chromatogram:
        {
            map<double, double> fullFileTIC;

            int sampleCount = wifffile_->getSampleCount();
            for (int i=1; i <= sampleCount; ++i)
            {

                int periodCount = wifffile_->getPeriodCount(i);
                for (int ii=1; ii <= periodCount; ++ii)
                {
                    //Console::WriteLine("Sample {0}, Period {1}", i, ii);

                    int experimentCount = wifffile_->getExperimentCount(i, ii);
                    for (int iii=1; iii <= experimentCount; ++iii)
                    {
                        ExperimentPtr msExperiment = wifffile_->getExperiment(i, ii, iii);

                        // add current experiment TIC to full file TIC
                        vector<double> times, intensities;
                        msExperiment->getTIC(times, intensities);
                        for (int iiii = 0, end = intensities.size(); iiii < end; ++iiii)
                            fullFileTIC[times[iiii]] += intensities[iiii];
                    }
                }
            }

            result->setTimeIntensityArrays(std::vector<double>(), std::vector<double>(), UO_minute, MS_number_of_counts);

            if (getBinaryData)
            {
                BinaryDataArrayPtr timeArray = result->getTimeArray();
                BinaryDataArrayPtr intensityArray = result->getIntensityArray();

                timeArray->data.reserve(fullFileTIC.size());
                intensityArray->data.reserve(fullFileTIC.size());
                for (map<double, double>::iterator itr = fullFileTIC.begin();
                     itr != fullFileTIC.end();
                     ++itr)
                {
                    timeArray->data.push_back(itr->first);
                    intensityArray->data.push_back(itr->second);
                }
            }

            result->defaultArrayLength = fullFileTIC.size();
        }
        break;

        case MS_SRM_chromatogram:
        {
            ExperimentPtr experiment = wifffile_->getExperiment(ie.sample, ie.period, ie.experiment);
            pwiz::wiff::Target target;
            experiment->getSRM(ie.transition, target);

            result->set(MS_dwell_time, target.dwellTime);
            result->precursor.isolationWindow.set(MS_isolation_window_target_m_z, ie.q1, MS_m_z);
            //result->precursor.isolationWindow.set(MS_isolation_window_lower_offset, ie.q1, MS_m_z);
            //result->precursor.isolationWindow.set(MS_isolation_window_upper_offset, ie.q1, MS_m_z);
            result->precursor.activation.set(MS_CID);
            result->precursor.activation.set(MS_collision_energy, target.collisionEnergy);
            result->precursor.activation.userParams.push_back(UserParam("MS_declustering_potential", lexical_cast<string>(target.declusteringPotential), "xs:float"));

            result->product.isolationWindow.set(MS_isolation_window_target_m_z, ie.q3, MS_m_z);
            //result->product.isolationWindow.set(MS_isolation_window_lower_offset, ie.q3, MS_m_z);
            //result->product.isolationWindow.set(MS_isolation_window_upper_offset, ie.q3, MS_m_z);

            result->setTimeIntensityArrays(std::vector<double>(), std::vector<double>(), UO_minute, MS_number_of_counts);

            vector<double> times, intensities;
            experiment->getSIC(ie.transition, times, intensities);
            result->defaultArrayLength = times.size();

            if (getBinaryData)
            {
                BinaryDataArrayPtr timeArray = result->getTimeArray();
                BinaryDataArrayPtr intensityArray = result->getIntensityArray();
                std::swap(timeArray->data, times);
                std::swap(intensityArray->data, intensities);
            }
        }
        break;
    }

    return result;
}


PWIZ_API_DECL void ChromatogramList_ABI::createIndex() const
{
    index_.push_back(IndexEntry());
    IndexEntry& ie = index_.back();
    ie.index = index_.size()-1;
    ie.id = "TIC";
    ie.chromatogramType = MS_TIC_chromatogram;

    pwiz::wiff::Target target;

    int periodCount = wifffile_->getPeriodCount(sample);
    for (int ii=1; ii <= periodCount; ++ii)
    {
        //Console::WriteLine("Sample {0}, Period {1}", sample, ii);

        int experimentCount = wifffile_->getExperimentCount(sample, ii);
        for (int iii=1; iii <= experimentCount; ++iii)
        {
            ExperimentPtr msExperiment = wifffile_->getExperiment(sample, ii, iii);

            for (int iiii = 0; iiii < (int) msExperiment->getSRMSize(); ++iiii)
            {
                msExperiment->getSRM(iiii, target);

                index_.push_back(IndexEntry());
                IndexEntry& ie = index_.back();
                ie.chromatogramType = MS_SRM_chromatogram;
                ie.q1 = target.Q1;
                ie.q3 = target.Q3;
                ie.sample = sample;
                ie.period = ii;
                ie.experiment = iii;
                ie.transition = iiii;
                ie.index = index_.size()-1;

                std::ostringstream oss;
                oss << "SRM SIC Q1=" << ie.q1 <<
                       " Q3=" << ie.q3 <<
                       " sample=" << ie.sample <<
                       " period=" << ie.period <<
                       " experiment=" << ie.experiment <<
                       " transition=" << ie.transition;
                ie.id = oss.str();
            }
        }
    }

    size_ = index_.size();
}


} // detail
} // msdata
} // pwiz


#endif // PWIZ_READER_ABI
