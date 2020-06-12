using System;
using System.Collections.Generic;
using Assets.Modules.SimpleLogging;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Models;
using CineastUnityInterface.Runtime.Vitrivr.UnityInterface.CineastApi.Processing;

namespace Assets.Scripts.Core
{
  public class TemporalRangeFilter : FilterStrategy
  {
    private Logger logger = LogManager.GetInstance().GetLogger(typeof(TemporalRangeFilter));

    private DateTime lowerBound;
    private DateTime upperBound;

    public TemporalRangeFilter(int fromYear, int uptoYear = 9999)
    {
      lowerBound = new DateTime(fromYear, 1, 1);
      upperBound = new DateTime(uptoYear, 12, 31);

      logger.Debug("Created with lowerBound=" + FormatBound(lowerBound) + ", upperBound=" + FormatBound(upperBound));
    }

    private string FormatBound(DateTime bound)
    {
      return bound.ToString("MM/dd/yyyy");
    }

    public override string ToString()
    {
      return string.Format("TemporalRangeFilter [lowerBound={0}, upperBound={1}]", FormatBound(lowerBound),
        FormatBound(upperBound));
    }

    public List<MultimediaObject> applyFilter(List<MultimediaObject> list)
    {
      List<MultimediaObject> ret = new List<MultimediaObject>();
      logger.Debug("Filtering. Original size: " + list.Count);

      foreach (MultimediaObject mmo in list)
      {
        if (mmo.datetime != null && mmo.datetime.Length > 0)
        {
          DateTime dt = DateTime.Parse(mmo.datetime);
          logger.Debug("Checking datetime source={0}, parsed={1}", mmo.datetime, dt.ToString("s"));
          if (lowerBound <= dt && dt <= upperBound)
          {
            ret.Add(mmo);
          }
        }
        else
        {
          logger.Warn(string.Format("Cannot check datetime of {0}. Adding it anyhow", mmo.id));
          ret.Add(mmo);
        }
      }

      logger.Debug("Finished filterinig. Remaining size: " + ret.Count);
      return ret;
    }
  }
}