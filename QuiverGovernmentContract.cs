/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NodaTime;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;

namespace QuantConnect.DataSource
{
    /// <summary>
    /// Government Contract by Agencies
    /// </summary>
    public class QuiverGovernmentContract : BaseDataCollection
    {
        private static readonly TimeSpan _period = TimeSpan.FromDays(1);

        /// <summary>
        /// Contract description
        /// </summary>
        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }
        
        /// <summary>
        ///     Awarding Agency Name
        /// </summary>
        [JsonProperty(PropertyName = "Agency")]
        public string Agency { get; set; }

        /// <summary>
        ///     Total dollars obligated under the given contract
        /// </summary>
        [JsonProperty(PropertyName = "Amount")]
        public decimal Amount { get; set; }
        
        /// <summary>
        /// The time the data point ends at and becomes available to the algorithm
        /// </summary>
        public override DateTime EndTime => Time + _period;

        /// <summary>
        /// Return the URL string source of the file. This will be converted to a stream
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>String URL of source file.</returns>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            return new SubscriptionDataSource(
                Path.Combine(
                    Globals.DataFolder,
                    "alternative",
                    "quiver",
                    "governmentcontracts",
                    $"{config.Symbol.Value.ToLowerInvariant()}.csv"
                ),
                SubscriptionTransportMedium.LocalFile,
                FileFormat.FoldingCollection
            );
        }

        /// <summary>
        /// Parses the data from the line provided and loads it into LEAN
        /// </summary>
        /// <param name="config">Subscription configuration</param>
        /// <param name="line">Line of data</param>
        /// <param name="date">Date</param>
        /// <param name="isLiveMode">Is live mode</param>
        /// <returns>New instance</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            var csv = line.Split(',');

            var parsedDate = Parse.DateTimeExact(csv[0], "yyyyMMdd") - _period;
            return new QuiverGovernmentContract
            {
                Symbol = config.Symbol,

                Description = csv[1],
                Agency = csv[2],
                Amount = Parse.Decimal(csv[3]),
                Time = parsedDate,
            };
        }

        /// <summary>
        /// Clones the data
        /// </summary>
        /// <returns>A clone of the object</returns>
        public override BaseData Clone()
        {
            return new QuiverGovernmentContract
            {
                Symbol = Symbol,
                Time = Time,
                Data = Data,

                Description = Description,
                Agency = Agency,
                Amount = Amount,
            };
        }

        /// <summary>
        /// Formats a string with QuiverGovernmentContract data
        /// </summary>
        /// <returns>string containing QuiverGovernmentContract information</returns>
        public override string ToString()
        {
            var niceString = Data.OfType<QuiverGovernmentContract>().Select(data => $"{Symbol}:: " +
                $"Description: {data.Description} " +
                $"Agency: {data.Agency} " +
                $"Amount: {data.Amount}");
            return $"{Time:yyyyMMdd}: [{string.Join(",", niceString)}]";
        }

        /// <summary>
        /// Indicates whether the data source is tied to an underlying symbol and requires that corporate events be applied to it as well, such as renames and delistings
        /// </summary>
        /// <returns>false</returns>
        public override bool RequiresMapping()
        {
            return true;
        }

        /// <summary>
        /// Indicates whether the data is sparse.
        /// If true, we disable logging for missing files
        /// </summary>
        /// <returns>true</returns>
        public override bool IsSparseData()
        {
            return true;
        }

        /// <summary>
        /// Gets the default resolution for this data and security type
        /// </summary>
        public override Resolution DefaultResolution()
        {
            return Resolution.Daily;
        }

        /// <summary>
        /// Gets the supported resolution for this data and security type
        /// </summary>
        public override List<Resolution> SupportedResolutions()
        {
            return DailyResolution;
        }

        /// <summary>
        /// Specifies the data time zone for this data type. This is useful for custom data types
        /// </summary>
        /// <returns>The <see cref="T:NodaTime.DateTimeZone" /> of this data type</returns>
        public override DateTimeZone DataTimeZone()
        {
            return DateTimeZone.Utc;
        }
    }
}
