# C# 8

本章要介紹的是 C# 8 的新增語法和改進之處，包括：

- [結構成員可宣告唯讀](#結構成員可宣告唯讀)
- [預設介面實作](#預設介面實作)
- [可為 Null 的參考型別](#可為-Null-的參考型別)

本章沒有包含下列 C# 8 的新功能：

- 可以為結構的成員加上 `readonly` 修飾詞。也就是可以指定結構中的個別成員是唯讀的。

## 結構成員可宣告唯讀

C# 8 開始可為結構的個別成員加上 `readonly` 修飾詞，藉以明確告訴編譯器：這是個唯讀的屬性或方法，亦即它不會改變物件的狀態。若不小心在某個宣告唯讀的函式中修改了物件的狀態，編譯器會立刻指出來。另一方面，我們只要看到某個方法前面有加上 `readonly`，就可以確定那個方法是絕對不會改變物件狀態的，這對我們日後閱讀與維護程式碼都有一些幫助。

> 除了閱讀本章，還有一個不錯的選擇是微軟的線上文件：〈[C# 8.0 的新功能](https://docs.microsoft.com/zh-tw/dotnet/csharp/whats-new/csharp-8)〉。其實，我在寫作時也有參考微軟線上文件與其範例，看到不錯的，便直接「借來用」，例如接下來的範例。

範例：

~~~~~~~~
public struct MyData
{
    public int Counter;

    public readonly int Increase => Counter + 1; // OK
    public readonly void Reset() => Counter = 0; // 編譯錯誤!
}
~~~~~~~~

第 5 行的 `Increase` 屬性只是傳回 `Counter` 加 1 的結果，而並未改變 `Counter`，也就是沒有改變物件的狀態，的確是唯讀操作。但下一行的 `Reset` 方法卻會把 `Counter` 改為 0，這就不符合 `readonly` 函式的要件，無法通過編譯。

接著來看一個比較微妙、需要我們特別留意的狀況：防禦複製（defensive copy）。延續剛才的範例，稍加修改：

~~~~~~~~
public struct MyData
{
    public int Counter;

    public int Increase => Counter++;
    public string IncreaseStr() => Increase.ToString();
}
~~~~~~~~

現在，`MyData` 結構中沒有任何唯讀方法，編譯也都沒問題。如果呼叫端有底下這樣一段程式碼：

~~~~~~~~
var d = new MyData();
Console.WriteLine(d.IncreaseStr()); // "0"
Console.WriteLine(d.IncreaseStr()); // "1"
~~~~~~~~

顯而易見，第 2 行程式碼的輸出結果是 "0"，第 3 行則是輸出 "1"。這是因為 `IncreaseStr` 方法使用了 `Increase` 屬性，而 `Increase` 屬性在回傳當前的 `Counter` 數值後，會把 `Counter` 內容加 1。

現在嘗試把 `IncreaseStr` 方法加上 `readonly` 看看：

~~~~~~~~
public struct MyData
{
    public int Counter;

    public int Increase => Counter++;
    public readonly string IncreaseStr() => Increase.ToString();
}
~~~~~~~~

然後再執行一遍剛才的呼叫端程式碼，結果這次會輸出兩個 "0"。為什麼呢？

修改後的程式碼，雖然可以通過編譯，但是在倒數第 2 行卻會出現編譯警告：

> Warning CS8656: Call to non-readonly member 'MyData.Increase.get' from a 'readonly' member results in an implicit copy of 'this'.	

意思是：唯讀方法（即此例的 `IncreaseStr`）裡面有去用到非唯讀的方法，對這種情況，編譯器無法判斷那些後續呼叫的非唯讀方法究竟會不會修改物件本身的狀態，故為了確保當下這個唯讀方法符合其 `readonly` 宣告的行為，編譯器在這裡會自動安插一個複製動作：把當前物件複製出一個分身，然後用那個分身來執行後續操作。因此，當呼叫端對同一個物件呼叫兩次 `IncreaseStr` 方法，即使每一次呼叫都有遞增 `Counter`，但其改變僅發生在「分身」，而不會去動到原有物件的狀態。這種由編譯器自動加入的複製物件行為，即所謂的防禦複製（defensive copy），目的是為了保證  `readonly` 方法的行為符合預期，亦即不會改變物件本身的狀態。

## 預設介面實作

預設介面實作（default interface implementation）是 C# 8 的新功能，它能夠讓你在介面中加入一些預設實作，包括方法、屬性、靜態欄位、巢狀型別等等。

那麼，C# 為什麼要增加這個語法呢？

我們知道，類別在實作某個介面的時候，一定要實作該介面所定義的所有成員。試著想像 C# 8.0 之前，也就是還沒有預設介面實作的時代，你設計的某個介面一旦對外公開了、有其他類別開始實作那個介面了，那麼介面就不可再更改 ——你不能修改既有的介面方法與屬性，也不能在那個介面中添加任何新的方法或屬性，否則原先已經實作該介面的類別便無法通過編譯（因為缺少了部分成員的實作）。這也是為什麼我們以前常聽到的一個原則：介面一旦公開了，就不可再修改。這形成了一種限制，導致介面的僵化，無法隨著時代和需求持續演進。

現在，C# 8 的預設介面實作放寬了上述限制，讓我們能夠對既有的介面添加新的成員。先來看一個簡單的例子：

~~~~~~~~
public interface ILogger
{
    void Log(string msg);
}
~~~~~~~~

如果某個類別要實作 `ILogger`，它就必須提供 `Log` 方法的實作：

~~~~~~~~
public class Logger
{
    void Log(string msg) 
    { 
        Console.WriteLine(msg); 
    }
}
~~~~~~~~

到目前為止，一切都沒問題，直到你想要在 `ILogger` 中增加新成員。此時，C# 8 的預設介面實作便可派上用場。假設你想要新增一個 `Error` 方法，便可以這樣寫：

~~~~~~~~
public interface ILogger
{
    // 這是原本我們熟悉的、沒有實作的介面方法。
    void Log(string msg); 
    
    // 此方法提供了預設實作。C# 8 以後才能這樣寫。
    void Error(Exception ex) 
    { 
        Console.WriteLine(ex); 
    }
}
~~~~~~~~

底下是用戶端的部分，跟以前的寫法完全一樣：

~~~~~~~~
ILogger logger1 = new Foo();
logger1.Error("....");

~~~~~~~~

注意不能這樣寫：

~~~~~~~~
Logger logger2 = new Logger();
logger2.Error(exception);  // 編譯錯誤!
~~~~~~~~

編譯錯誤的原因是，預設方法 `Error` 只存在介面裡，而不會被類別 `Logger` 繼承下來。

除了提昇介面的向後相容能力（backward compatibility），「預設介面實作」也帶來了更多新語法，例如在介面中定義常數、靜態欄位、巢狀型別等等。參考以下範例：

~~~~~~~~
public interface ILogger
{
    void Log(string msg)     
    
    // 預設實作。
    void Error(Exception ex);
    { 
        // 保存或輸出錯誤訊息。
    }

    // 巢狀型別
    enum LogLevel
    {
        Debug,
        Information,
        Warning,
        Error,
    };
	
    // 使用了巢狀型別的屬性
    LogLevel Level { get; set; } 

    // 靜態成員
    static ILogger DefaultLogger;
}
~~~~~~~~

實作 `ILogger` 的類別，必須提供 `Log` 方法，至於 `Error` 方法則可有可無，原因正如前面提過的，介面本身已經提供 `Error` 方法的預設實作。參考以下範例：

~~~~~~~~
public class Logger : ILogger
{
    public ILogger.LogLevel Level { get; set; } = ILogger.LogLevel.Debug;

    public void Log(string msg)
    {
        Console.WriteLine(msg);
    }	
}   
~~~~~~~~

另外也值得留意的是第 3 行使用了巢狀型別 `ILogger.LogLevel` 來定義屬性和設定初始值。

這些介面成員也都可以加上存取範圍修飾詞，如上例的巢狀型別，你可以根據實際需要來決定是否替它加上 `private`、`protected`、或 `public` 修飾詞（介面中的成員預設為 public）。

## 可為 Null 的參考型別

對於熟悉 C# v1 至 v7.x 的人來說，初次聽到「可為 null 的參考型別」（Nullable Reference Types）可能會覺得奇怪：宣告為參考型別的變數不是本來就可以為 null 嗎？而且如果沒有給值，其預設值就是 null（未指向任何物件）。為什麼還要特別強調「可為 null 的參考」呢？

在 C# 8.0 之前，由於參考型別的變數預設可為 null，而且在執行時期隨時都有可能為 null，所以我們以往在寫 C# 程式的時候，經常得在程式各處寫一些安全防護的程式碼：如果某變數不是 null 才繼續做某件事。例如：

~~~~~~~~
static int StrLen(string text)
{
    return text == null? 0 : text.Length;
}
~~~~~~~~

就上例來說，我們在寫 `StrLen` 函式時，並沒有辦法確定傳入的參數 `text` 究竟有沒有值；如果不先檢查變數是否為 null 就使用它的屬性或方法，那麼當程式執行時，只要呼叫端傳入 null，就會引發 `NullReferenceException` 類型的錯誤。

然而，變數為 null 的情形可能到處都是，防不勝防，如果在編譯時期就能盡量避免這類潛在問題，應用程式必然更加穩固，開發人員也能少寫一些重複瑣碎的程式碼，如此不僅減輕了人的負擔，程式碼也更簡潔、更能明白呈現程式碼的意圖。這便是 C# 8 加入 Nullable References 的主要原因。

 一旦你決定在程式中使用 C# 8 的這項新功能，在宣告參考型別的變數時，若允許它為 null，則必須在型別後面附加一個問號（`?`）。請看底下這個簡單的範例：

~~~~~~~~
string str1 = "hello"; // str1 是不可為 null 的字串
string? str2 = null;   // str2 是可為 null 的字串
~~~~~~~~

你會發現，原本常用的語法（上面範例的第一行），在加入 Nullable Reference Types 功能之後被賦予了新的意義；換言之，以往的參考型別是預設可為 null，現在變成預設不可為 null 了。就語意而言，這是蠻大的改變，而且必然對既有的程式碼帶來不少衝擊，故在預設情況下，Nullable Reference Types 功能是關閉的。

> 為了方便說明與閱讀，有時我會使用比較簡短的英文「Nullable References」來代表 Nullable Reference Types。

### 開啟 Nullable Reference Types 功能

讓我們先試試看，在使用 C# 8 來編譯程式的情況下，不做任何額外設定，以下程式碼能夠通過編譯嗎？

~~~~~~~~
string? str2 = null;  // str2 是可為 null 的字串
~~~~~~~~

可以通過編譯，但是伴隨警告：

> CS8632: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

這是因為，如前面提過的，由於 C# 8 的 Nullable Reference Types 是重大改變，對開發人員和既有程式碼都會產生不少衝擊，所以此功能預設是關閉的。當我們想要使用「可為 null 的參考型別」時，就必須明白指示編譯器要開啟這項功能。

為了讓開發人員能夠更彈性地應付各種狀況，C# 提供了兩種面向的控制開關：

- **nullable annotation context**：是否啟用 Nullable References 語法。（註：多數文件把 annotation 翻譯為「注釋」，而我選擇把 annotation 稱作「語法」，主要是方便理解。）
- **nullable warning context**：是否啟用 Nullable References 相關的編譯警告。

為什麼這兩種開關都以「context」（環境、上下文）來命名呢？我想這大概是因為 C# 可以讓我們針對任意範圍的（甚至只有一行）程式碼來控制與 Nullable References 語法有關的編譯行為。

再重複一次：預設情況下，Nullable References 語法和編譯警告都是關閉的（disabled）。也就是說，即使不修改任何程式碼，你的既有 C# 專案也能跟以往一樣順利通過編譯，而且不會出現與 Nullable References 有關的警告訊息。

接著來看看如何控制這些開關。

### 在個別檔案中使用 #nullable 指示詞

你可以在程式碼的任何地方使用 `#nullable` 指示詞來啟用或關閉 Nullable References 語法或警告（即剛才提過的 `annotation context` 和 `warning context`）。例如，在一個 C# 程式檔案的最上方或者 `using` 陳述式下方加入一行 `#nullable enable`，這表示整個檔案裏面的程式碼都會啟用 Nullable References 語法。

你也可以只對局部程式碼區塊啟用 Nullable References 語法，作法是在需要啟用新語法的地方加上 `#nullable enable`，然後在不需要此語法的地方加上 `#nullable disable`，或者使用 `#nullable restore` 來回復至專案層級的設定（稍後會介紹）。參考以下範例：

~~~~~~~~
#nullable enable    // 啟用 nullable 語法和警告
    string? str1 = null;
#nullable disable   // 關閉 nullable 語法和警告
    string str2 = null;
~~~~~~~~

上列程式碼可以順利通過編譯，而且沒有任何警告訊息。其中 str1 是可為 null 的字串，而 str2 也是可為 null 的字串；差別只在於前者使用的是 C# 8 的 Nullable Refernce Types 語法，後者則為舊版 C# 語法。如果把這兩行程式碼對調，則會各自引發編譯警告：

~~~~~~~~
#nullable enable    
    string str1 = null;   // 編譯警告 CS8600
#nullable disable   
    string? str2 = null;  // 編譯警告 CS8632
~~~~~~~~

編譯警告 CS8600 的訊息是：

> 中文：正在將 Null 常值或可能的 Null 值轉換為不可為 Null 的型別。
> 
> 英文：Converting null literal or possible null value to non-nullable type.

編譯警告 CS8632 的內容是：

> 中文：可為 Null 的參考型別註釋應只用於 '#nullable' 註釋內容中的程式碼。
> 
> 英文：The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

經過前面的說明，相信你應該已經能夠理解為什麼那兩行程式碼會出現編譯警告了。

底下列出 #nullable 指示詞的各種控制組合：

- `#nullable disable`：關閉 nullable 語法和編譯警告。（這是預設情形）
- `#nullable enable`：開啟 nullable 語法和編譯警告。
- `#nullable restore`：從這裡開始套用專案層級的 nullable 設定。
- `#nullable disable annotations`：關閉 nullable 語法。
- `#nullable enable annotations`：開啟 nullable 語法。
- `#nullable restore annotations`：從這裡開始套用專案層級的 nullable 語法開關。
- `#nullable disable warnings`：關閉 nullable 相關的編譯警告。
- `#nullable enable warnings`：開啟 nullable 相關的編譯警告。
- `#nullable restore warnings`：從這裡開始套用專案層級的 nullable 編譯警告開關。

有了這些控制開關，你就可以採取循序漸進的方式來把既有的 C# 專案逐漸修改成 C# 8 的 Nullable References 語法。比如說，先針對少數幾個檔案加入 `#nullable enable`，感受一下啟用新語法之後，要花多少工夫來修改程式碼，才能消除所有的編譯警告。等到熟練了，覺得更有把握了，再把修改範圍擴及更多 C# 程式檔案。

### 專案與方案層級的設定

除了檔案層級的 `#nullable` 指示詞，你也可以在 .csproj 檔案裡面加入 `<Nullable>enable</Nullable>` 來讓整個專案都啟用 Nullable References 語法。如下所示（第 5 行）：

~~~~~~~~
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp3.1</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
~~~~~~~~

與 `#nullable` 指示詞類似，`<Nullable>` 元素除了指定為 `enable` 之外，你也可以在這裡使用 `disable`、 `warnings`、`annotations`。

你甚至可以把控制範圍擴及整個方案（solution），作法是在方案的根目錄下建立一個名為 Directory.Build.props 的檔案，內容則和 .csproj 裡面的寫法一樣。參考底下的範例：

~~~~~~~~
<Project>
 <PropertyGroup>
    <Nullable>enable</Nullable>
    <RunAnalyzersDuringBuild>true</RunAnalyzersDuringBuild>
    <RunAnalyzersDuringLiveAnalysis>true</RunAnalyzersDuringLiveAnalysis>
 </PropertyGroup>
</Project>
~~~~~~~~


> **Nullable References 語法的編譯警告列表**
>
> 如果你想要知道 C# 編譯器在檢查 Nullable References 語法時的規則與警告訊息，可以參考這個頁面：[CsharpNullableTypeRules.md](https://gist.github.com/huanlin/e6aebce9d13340a94ba7a6868e5c790c)。這是利用 [Cezary Piątek 提供的程式碼](https://gist.github.com/cezarypiatek/73bc42beda006bf7890d9ccc7263da03)所產生的結果，我只是把程式裡面的 "en-US" 改為 "zh-TW" 而已。
> 
> 如果你想要更進一步全面採用 Nullable References 語法，可以考慮使用 [EditorConfig 檔案](https://docs.microsoft.com/zh-tw/visualstudio/ide/create-portable-custom-editor-options)來把相關編譯警告提升至「錯誤」等級。


## 重點整理

- Nullable Reference Types 對既有程式會產生不小衝擊，故此功能預設為關閉。
- C# 提供了兩種面向的控制開關：（1）`nullable annotation context`：是否啟用 Nullable References 語法；以及（2）`nullable warning context`：是否啟用 Nullable References 相關的編譯警告。
- 我們可以在 .csproj 裡面使用 `<Nullable>enable</Nullable>` 來進行專案層級的設定，也可以在單一檔案裡面透過編譯指示詞 `#nullable enable` 來啟用此功能，或者用 `#nullable disable` 將它關閉，又或者使用 `#nullable restore` 回復至專案層級的設定。在 solution 根目錄下的 Directory.Build.props 檔案中的設定則可以套用至整個 solution 的全部專案。
