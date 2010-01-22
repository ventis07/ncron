﻿/*
 * Copyright 2009, 2010 Joern Schou-Rode
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

namespace NCron.Service
{
    internal class QueueEntry : IComparable<QueueEntry>
    {
        private readonly Func<ICronJob> _jobConstructor;
        private readonly ISchedule _schedule;
        private readonly DateTime _nextOccurence;

        public DateTime NextOccurence
        {
            get { return _nextOccurence; }
        }

        public QueueEntry(Func<ICronJob> jobConstructor, ISchedule schedule, DateTime baseTime)
        {
            _jobConstructor = jobConstructor;
            _schedule = schedule;
            _nextOccurence = schedule.GetNextOccurrence(baseTime);
        }

        public int CompareTo(QueueEntry other)
        {
            return NextOccurence.CompareTo(other.NextOccurence);
        }

        public ICronJob GetJobInstance()
        {
            return _jobConstructor();
        }

        public QueueEntry GetSubsequentEntry()
        {
            return new QueueEntry(_jobConstructor, _schedule, _nextOccurence);
        }
    }
}
