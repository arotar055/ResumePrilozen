﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResumePrilozen
{
    public class DetailWindowViewModel
    {
        public ResumeModel ChosenResume { get; }

        public DetailWindowViewModel(ResumeModel resume)
        {
            ChosenResume = resume;
        }
    }
}
