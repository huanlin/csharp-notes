using CustomInterpolStringHandler;

var logger = new MyLogger() { Enabled = true };
logger.Log("Hello");

var date = DateTime.Now;
logger.Log($"今天是 {date.Month} 月 {date.Day} 日");

logger.LogDebug($"今天是 {date.Month} 月 {date.Day} 日"); // 擴充方法