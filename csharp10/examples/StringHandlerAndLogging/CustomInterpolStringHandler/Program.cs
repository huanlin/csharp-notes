using CustomInterpolStringHandler;

var logger = new MyLogger() { Enabled = true };
logger.Log("Hello");

var date = DateTime.Now;
logger.Log($"今天是 {date.Year} 年 {date.Month} {date.Day} 日");

logger.LogDebug($"今天是 {date.Year} 年 {date.Month} {date.Day} 日");