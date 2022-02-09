# C# 6

本章要介紹的是 C# 6 新增的語法，包括：

 - [`nameof` 運算子](#nameof-運算子)
 - [字串插值](#字串插值)
 - [唯讀自動實作屬性](#唯讀自動實作屬性)
 - [自動屬性的初始設定式](#自動屬性的初始設定式)
 - [以表示式為本體的成員](#以表示式為本體的成員)
 - [索引初始設定式](#索引初始設定式)
 - [`using static` 陳述式](#using-static-陳述式)
 - [例外篩選條件](#例外篩選條件)
 - [ `catch` 和 `finally` 區塊中的 `await`](#catch-和-finally-區塊中的-await)

---

 ## `nameof` 運算子

C# 6 新增的 `nameof` 關鍵字可用來取得某符號的名稱，例如型別、變數、物件成員等等。請看底下這個簡單範例：

~~~~~~~~
string firstName = "Joey";
string varName = nameof(firstName);  // 編譯結果：varName = "firstName"
~~~~~~~~

程式碼右邊的註解已經透露，`nameof` 是作用於編譯時期，而非執行時期。

那麼，它可以用在哪裡呢？

比如說，為了防錯，我們經常在函式中預先檢查參數值是否合法，並將不合法的參數透過拋出例外（exception）的方式通知呼叫端（或者寫入 log 檔案以供將來診斷問題）。像這樣：

~~~~~~~~
void LoadProduct(string productId)
{
    if (productId == null)
    {
        throw new ArgumentNullException("productId"));
    }
    ....
}
~~~~~~~~

如此一來，當程式出錯時，用戶端就能輕易知道是哪個參數不對。問題是，程式中的參數名稱是寫死的字串值（`"productId"`），將來萬一要修改參數名稱，稍不注意就會漏改這些字串。於是，即使煞費周章，仍有人選擇在執行時期動態取得參數名稱，只為了避免在程式中寫一堆固定的字串。其實，參數名稱在編譯時期就已經決定了，何不由編譯器代勞，既省事又不犧牲執行效率？

現在，C# 6 提供了 `nameof` 表示式，正好可以解決這個問題。於是剛才的範例可以改寫成這樣：

~~~~~~~~
void LoadProduct(string productId)
{
    if (productId == null)
    {
        throw new ArgumentNullException(nameof(productId));
    }
    ....
}
~~~~~~~~

其中的 `nameof(productId)` 表示式在經過編譯之後，結果就等於手寫的固定字串 `"productId"`。

## 字串插值

在 C# 6 之前，需要格式化字串的時候，大多呼叫 .NET Framework 提供的字串格式化方法 `String.Format(...)`。在 C# 6 開始提供了另一種更方便的格式化字串語法，名曰「字串插值」（string interpolation）。

字串插值的基本語法，是在雙引號包住的字串前面加上一個 `'$'` 字元，而在字串內容的部分使用一對大括弧 `{}` 來包住一個變數名稱。如下所示：

{line-numbers=off, lang=text}
    $"{變數名稱:格式規範}"

其中的「格式規範」就跟 `String.Format()` 方法所使用的格式規範字元一樣。一個格式字串裡面可以有多對大括弧，用來帶入不同的變數值，而沒有被大括弧包住的部分則維持不變。

參考底下的範例：

~~~~~~~~
string firstName = "Michael";
string lastName = "Tsai";
int salary = 22000;

string msg1 = String.Format("{0} {1} 的月薪是 {2:C0}", firstName, lastName, salary);
string msg2 = $"{firstName} {lastName} 的月薪是 {salary :C0}";
Console.WriteLine(msg1);
Console.WriteLine(msg2);
~~~~~~~~

結果兩次輸出的字串值都相同，如下所示：

~~~~~~~~
Michael Tsai 的月薪是 22,000
Michael Tsai 的月薪是 22,000
~~~~~~~~

跟既有的 `String.Format()` 方法比較，我覺得字串插值的寫法更容易解讀，更容易想像最終格式化的結果。原因在於，解讀 `String.Format()` 時，我們必須把右邊的參數依序帶入左邊的格式字串；如果參數很多，有時還會對不上，導致順序錯置。新的字串插值語法則是直接在格式字串裡面填入變數名稱，不僅直觀，而且不會有弄錯順序的問題。

字串插值跟 `nameof` 表示式一樣都是語法糖，是編譯時期的魔法。明白地說，編譯器會把字串插值的語法編譯成 `String.Format()` 方法呼叫。底下是更多字串插值的範例，右邊註解則是編譯後的結果。

~~~~~~~~
$"姓名 = {myName}"    // String.Format("姓名 = {0}", myName)
$"小時 = {DateTime.Now:hh}"  // String.Format("小時 = {0:hh}", DateTime.Now)
$"{{測試大括弧}}"      // String.Format("{{測試大括弧}}")
$"{{秒 = {DateTime.Now :ss}}}" // String.Format("{{秒 = {0:ss}}}", DateTime.Now)
~~~~~~~~

需注意的是，兩個連續的大括弧代表欲輸出一個大括弧字元。故第三個例子的執行結果為 "{測試大括弧}"。

## `Null` 條件運算子

保險起見，在需要存取某物件的屬性之前，我們通常會先檢查該物件是否為 `null`，以免程式執行時拋出例外（`NullReferenceException`）。一般常見的寫法如下：

~~~~~~~~
static void NullPropagationDemo(string s)
{
    if (s != null && s.Length == 4) // 只有當 s 不為空時才存取它的 Length 屬性。
    {
        // Do something.
    }
}
~~~~~~~~

C# 6 新增了 `null` 條件運算子語法，讓你用更簡短的語法達到同樣效果：先判斷物件是否為 `null`，不是 `null` 才會存取其成員。它的寫法是在變數後面跟著一個問號，然後是存取成員名稱的表示式。參考以下範例：

~~~~~~~~
static void NullPropagationDemo(string s)
{
    if (s?.Length == 4) // 只有當 s 不為空時才存取它的 Length 屬性。
    {
        // Do something.
    }
}
~~~~~~~~

更多範例：

~~~~~~~~
int? length = empList?.Length; // 若 empList 是 null，則 length 也是 null。
Employee emp = empList?[0];    // 若 empList 是 null，則 obj 也是 null。
int length = empList?.Length ?? 0;  // 若 empList 是 null，則 length 是 0。
string name = empList?[0].FullName; // 若 empList 是 null，則 name 是 null。
~~~~~~~~

## 唯讀自動實作屬性

請看以下程式片段：

~~~~~~~~
// C# 3：自動實作屬性（免寫私有欄位）。
public class Employee
{
    public string ID { get; private set; } // 外界可讀，但不可改變此屬性值。

    public Employee(string id) // 建構子
    {
        ID = id; // 設定自動屬性 ID 的初始值。
    }
}
~~~~~~~~

其中 `ID` 是個自動實作屬性（或簡稱「自動屬性」），它可讓外界讀取，但是只有類別本身才能修改其值。所謂的類別本身，當然包含類別的建構子和方法。可是，如果我們希望這個唯讀屬性只允許在建構子中設定一次初始值，以後就再也不能修改了——即使在類別的其他方法中也不能修改，這種情況要怎麼寫呢？ C# 5 沒辦法做到——除非使用唯讀的私有欄位，但程式碼寫起來比較囉嗦。

C# 6 在這方面做了改進，增加了「**唯讀自動實作屬性**」（read-only automatically implemented properties）語法，或簡稱「**唯讀自動屬性**」。請看以下範例：

~~~~~~~~
// C# 6：唯讀自動屬性。
public class Employee
{
    public string ID { get; } // 沒有 setter，這是個唯讀自動屬性。

    public Employee(string id)
    {
        ID = id; // 設定唯讀自動屬性的初始值。
    }

    private void ChangeID(int id)
    {
        ID = id;  // 編譯失敗：ID 是唯讀屬性!
    }
}
~~~~~~~~

如您所見，此處有兩個重點：

* 屬性 ID 只有 getter，沒有 setter 了。它是個唯讀自動屬性。
* 由於 ID 是唯讀屬性，因此即使是類別自己也無法修改 ID 的值——建構子除外。

### 明確實作介面的唯讀屬性

現在，你已經知道唯讀自動屬性的語法了，且讓我們來看一個有點微妙、可能令你驚訝的問題。請看底下這段程式碼：

~~~~~~~~
interface IEmployee
{
    int Salary { get; }   // 唯讀屬性。
}

class Employee : IEmployee
{
    public int Salary { get; } // 隱含實作 IEmployee.Salary 屬性。

    public Employee()
    {
        Salary = 70000; // 在建構函式中初始化唯讀屬性。
    }
}
~~~~~~~~

說明：

* 介面 `IEmployee` 只定義了一個成員：`Salary`，它是個唯讀屬性。
* 類別 `Employee` **隱含實作**了 `IEmployee.Salary` 屬性。由於它是唯讀自動屬性，故在建構函式中設定其初始值。

那麼，如果我們想要改成**明確實作**（explicitly implement）介面的寫法，直覺上可能會這樣寫：

~~~~~~~~
class Employee : IEmployee
{
    int IEmployee.Salary { get; } // 明確實作 IEmployee.Salary 屬性。

    public Employee()
    {
        Salary = 70000; // 編譯失敗: 此處無法使用 Salary!
    }
}
~~~~~~~~

倒數第三行無法通過編譯，錯誤訊息是：

> The name 'Salary' does not exist in the current context.
> （在目前的區塊裡面不存在 'Salary' 這個東西。）

這是因為 C# 語法規定，採用「明確實作介面」的時候，不能在類別裡面直接以名稱來存取介面的成員。

那麼，把發生錯誤的那行程式碼改成這樣呢：

~~~~~~~~
(this as IEmployee).Salary = 70000;
~~~~~~~~

這也行不通，編譯器會告訴你：

> Property or indexer 'IEmployee.Salary' cannot be assigned to -- it is read only.

因為 `Salary` 本來就是定義成唯讀屬性，不可修改。

可是，回頭再看一下前面的範例，當我們採用隱含實作介面的寫法時，`Salary` 也是定義成唯讀屬性，卻可以在建構函式中設定初始值。顯然，「在建構函式中可以設定唯讀屬性」這項語法規則，一旦碰到明確實作介面的場合就行不通了。

若一定要使用明確實作介面的寫法，我們可以繞個彎，用唯讀的私有欄位來保存屬性值，便可解決。如下所示：

~~~~~~~~
class Employee : IEmployee
{
    private readonly int _salary;   // 唯讀的私有欄位。

    int IEmployee.Salary    // 明確實作 IEmployee.Salary 屬性。
    {
        get { return _salary; }
    }

    public Employee()
    {
        _salary = 70000;
    }
}
~~~~~~~~

## 自動屬性的初始設定式

在 C# 6 之前，自動屬性的初始值只能透過建構子來設定，而無法在宣告屬性的時候就設定初始值。C# 6 對此做了改進，提供了「自動屬性初始設定式」（auto-property initializers）語法，讓我們能夠在宣告屬性的時候就一併設定初始值。如以下範例所示：

~~~~~~~~
public class Employee
{
    public string ID { get; private set; } = "A001";
    public string Name { get; set; } = "Michael";
    public int Age { get; } = 20;
}
~~~~~~~~

此語法的主要特色，是在定義自動屬性的時候用 `=` 運算子來加上賦值敘述，以設定該屬性的初始值。需注意的是，此賦值語法只能用於自動實作屬性；若用在非自動屬性，編譯時會報錯。所以底下的程式碼無法通過編譯：

~~~~~~~~
public class Employee
{
    private string _name;

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    } = "另壺沖"  // 編譯錯誤! 只有自動屬性才允許初始設定式。
}
~~~~~~~~

另外，自動屬性的初始設定式無法透過 `this` 來存取該類別的其他成員。也就是說，如果想要在自動屬性的初始設定式中呼叫該類別的方法，則該方法必須是靜態方法。請看以下範例與註解中的說明：

~~~~~~~~
public class Employee
{
    public int Age { get; } = this.GetAge(); // 編譯失敗！不可呼叫 instance 成員。
    public int Salary { get; } = GetSalary();  // OK! 可以呼叫靜態方法。

    int GetAge() { return 20; }
    static int GetSalary() { return 20000; } // 注意：這是個靜態方法。
}
~~~~~~~~

在進入下一個議題之前，我想再補充一下**唯讀自動屬性**所提供的「不可變性」（immutability）在寫程式的時候有什麼好處。請看底下的範例：

~~~~~~~~
public class Employee
{
    public List<string> Addresses { get; } // 唯讀自動屬性
    }

    public Employee() // 在建構子中初始化唯讀自動屬性
    {
        Addresses = new List<string>();
    }

    public void DoSomethingStupid()
    {
        Addresses = null; // 編譯失敗：不允許修改。
    }
}
~~~~~~~~

屬性 `Addresses` 是個唯讀自動屬性，一旦初始化完成，就不能在其他地方修改。於是乎，外界可以取得 `Addresses` 串列，並且呼叫它的 `Add` 或 `Remove` 方法來增加或刪除元素，但是無法將這個 `Addresses` 屬性設定成 `null` 或指向其他串列。參考以下範例：

~~~~~~~~
public static void Main()
{
    var emp = new Employee();
    emp.Addresses.Add("嘉義市大馬路 123 號"); // OK!

    emp.Addresses = new List<string>(); // 編譯失敗：不允許修改。
}
~~~~~~~~

> 範例原始碼：https://dotnetfiddle.net/9TINL9

## 以表示式為本體的成員

C# 6 新增了「以表示式為本體的成員」語法，英文是 **expression-bodied members**。意思是：以表示式來作為成員方法的本體。嗯……我們還是直接看程式碼比較清楚吧。

C# 6 之前：

~~~~~~~~
class BeforeCSharp6
{
    private DateTime startTime = DateTime.Now;

    public int ElapsedSeconds
    {
        get
        {
            return (DateTime.Now - startTime).Seconds;
        }
    }
}
~~~~~~~~

此範例的唯讀屬性 `ElapsedSeconds` 的值是動態計算出來的，代表從 `startTime` 開始之後到目前為止經過了幾秒鐘。

C# 6 之後，可以這麼寫：

~~~~~~~~
class NewInCsharp6
{
    private DateTime startTime = DateTime.Now;

    public int ElapsedSeconds => (DateTime.Now - startTime).Seconds;
}
~~~~~~~~

這裡使用了 C# 6 新增的 expression-bodied members 語法。如您所見，程式碼變得更簡短了。原本的唯讀屬性 `ElapsedSeconds` 的 `get` 區塊拿掉了，取而代之的是屬性名稱後面跟著一個以 `=>` 符號開始的 lambda 表示式——骨子裡，編譯器會把它編譯成一個 `get` 方法，就跟先前的 `BeforeCSharp6` 類別裡面的寫法類似。

> 儘管使用了 `=>` 符號，expression-bodied members 語法並非 lambda 表示式，而且跟匿名委派也沒啥關係；它只是單純告訴編譯器：請建立一個唯讀屬性，而且它的回傳值會是接在 `=>` 後面的表示式的運算結果。它就只是語法糖而已。

要注意的是，這個表示式只能有一行敘述，而不能包含多行程式碼的區塊。因此，你不能這麼寫：

~~~~~~~~
public int ElapsedSeconds =>
{
    return (DateTime.Now - startTime).Seconds; // 編譯失敗！
}
~~~~~~~~

此外，「以表示式為本體的成員」可以是屬性、索引子（indexer）或方法，而且有沒有回傳值都可以。底下是更多範例：

~~~~~~~~
public class Demo
{
    public string FullName =>
            this.FirstName + " " + this.LastName;
    public Color GetColor(int r, int g, int b) =>
            Color.FromArgb(r, g, b);
    public void Log(string msg) => Console.WriteLine(msg);
    public static string HowCold(int temperature) =>
            temperature > 17 ? "不冷" : "挺冷";
    public Employee this[int id] =>    // 這式唯讀索引子。
            this.FindEmployee(id);
}
~~~~~~~~

顯然，對於那些只包含一行程式碼的屬性或方法，我們可以盡量使用「以表示式為本體的成員」語法來撰寫，讓程式碼更簡潔一些。

> **視力測驗**
>
> 看一下這兩行程式碼：
>
>        public double PI = 3.14;   // 可讀寫的公開欄位
>        public double PI => 3.14;  // 唯讀的公開屬性
>
> 兩者皆可編譯，寫法只差一個字元，但意義卻不一樣。在寫程式或審閱別人的程式碼時，眼睛可能得睜大一點，以免漏掉 `>` 符號。

## 索引初始設定式

以常用的 `Dictionary<TKey, TValue>` 集合為例，在 C# 6 之前的初始設定式的寫法如下：

~~~~~~~~
var weeks = new Dictionary<int, string>()
{
    {1, "星期一"},
    {2, "星期二"},
    {3, "星期三"}
};
~~~~~~~~

到了 C# 6，只要集合類型本身有支援索引子，便可使用索引子的語法來初始化集合元素。如下所示：

~~~~~~~~
var weeks = new Dictionary<int, string>()
{
    [1] = "星期一",
    [2] = "星期二",
    [3] = "星期三"
};
~~~~~~~~

底下再借用第 1 章的範例，作為補充。其初始設定式使用了自訂類別 `Student`：

~~~~~~~~
// C# 6 之前
var students1 = new Dictionary<int, Student>()
{
    { 101, new Student { Name="黃濤" } },
    { 102, new Student { Name="藍波" } }
}

// C# 6 之後
var students2 = new Dictionary<int, Student>()
{
    [101] = new Student { Name="黃濤" },
    [102] = new Student { Name="藍波" }
};
~~~~~~~~

兩相對照，是不是更清楚它們在寫法上的差異了呢？你可以選擇自己喜歡的寫法。

> 我是偏好索引子的寫法，因為程式碼可以少寫一對大括弧，看起來更清楚。

## `using static` 陳述式

在 C# 6 之前，只要有引用（`using`）某類別所屬的命名空間，便可使用「類別名稱.成員名稱」的語法來存取該類別的靜態成員。例如：

~~~~~~~~
using System;      // 引用 System 命名空間（的所有型別）
using System.IO;   // 引用 System.IO 命名空間（的所有型別）

class BeforeCSharp6
{
    void Demo()
    {
        Console.WriteLine("Hello!");
        File.WriteAllText(@"C:\test.txt", "Hello!");
    }
}
~~~~~~~~

到了 C# 6，新增的 `using static` 陳述式可以讓存取靜態成員的寫法更簡潔。它的語法是：

    using static 某型別;

關鍵字 `using static` 本身已經揭示，其目的是要讓你更方便地「使用靜態成員」。說得更明白些，也就是匯入某型別的所有靜態成員，以便在目前的程式檔案中直接使用那些靜態成員，而無須指涉型別名稱。

於是，剛才的範例可以改寫成這樣：

~~~~~~~~
using static System.Console;  // 匯入 System.Console 類別的所有靜態成員
using static System.IO.File;  // 匯入 System.IO.File 類別的所有靜態成員

class NewInCSharp6
{
    void Demo()
    {
        WriteLine("Hello!");  // 省略了 "Console."
        WriteAllText(@"C:\test.txt", "Hello!"); // 省略了 "File."
    }
}
~~~~~~~~

不難想像，當你需要在程式中大量存取某類別的靜態成員時，`using static` 的確可以讓我們少打一些字。

不過，這個新口味的語法糖恐怕不見得人人都愛。怎麼說呢？

譬如 `WriteLine` 這類常見的方法，由於不只一個類別提供此方法，使用時可能會產生名稱衝突而導致編譯失敗。另一個可能的問題是，如果大量使用 `using static` 來匯入許多類別的靜態成員，雖然寫程式的時候可以少打一些字，但是在閱讀別人的程式碼的時候可能會有疑問：這個靜態方法（或屬性）究竟來自何處？

然而在某些場合，省略類別名稱的寫法卻是同時兼具簡潔美觀、易讀易懂的優點。例如 `Math` 類別。試比照底下兩行程式碼的寫法：

~~~~~~~~
var value1 = Math.Sqrt(Math.Sin(90) + Math.Cos(45)); // using System
var value2 = Sqrt(Sin(90) + Cos(45));     // using static System.Math
~~~~~~~~

第一行程式碼是一般的靜態方法呼叫。第二行程式碼則是使用了 `using static` 的寫法。就我的感覺，第二行程式碼的寫法不僅更簡潔，而且不影響程式碼的理解，甚至更容易閱讀。

> 當靜態方法的名稱非常明確、普遍，而且一看就知道它的功用與所屬型別，那麼它通常適合以 `using static` 的方式匯入。

值得一提的是，由於 `using static` 作用的對象是型別（而不是命名空間），因此它還可以用來限定只匯入某類別的擴充方法（extension methods），而不是像一般的 `using` 陳述式那樣匯入某命名空間的全部擴充方法。當你在 C# 程式檔案中 `using` 了許多命名空間，而其中有兩個命名空間碰巧包含了相同名稱的擴充方法，此時亦可考慮使用 `using static` 匯入指定類別的方式來嘗試解決名稱衝突的問題。

## 例外篩選條件

在處理例外狀況的時候，我們可能會想要針對捕捉到的例外再進一步依照其內部詳細錯誤資訊來做個別處理。比如說，當 `catch` 區塊捕捉到了 `SqlException`，我們也許會想知道，這次發生的資料庫操作錯誤，究竟是因為主鍵重複的緣故，還是發生死結（deadlock）或其他原因；如果是主鍵重複或死結，就撰寫額外的邏輯來處理。在 C# 6 之前，我們可能會這樣寫：

~~~~~~~~
try
{
    // 執行資料異動。
}
catch (SqlException ex)
{
    if (ex.Number == 2627)
    {
        // 主鍵重複!
    }
    else if (ex.Number == 1205)
    {
        // Deadlock!
    }
    else throw;
}
~~~~~~~~

從 C# 6 開始，例外處理的 `catch` 區塊可以一併指定篩選條件，語法是在 `catch` 後面加上以 `when` 開頭的條件式，而只有當 `when` 子句的條件式成立才會進入那個 `catch` 區塊。所以剛才的例子可以改成：

~~~~~~~~
try
{
    // 執行資料異動。
}
catch (SqlException ex) when (ex.Number == 2627)
{
    // 主鍵重複!
}
catch (SqlException ex) when (ex.Number == 1205)
{
    // Deadlock!
}
~~~~~~~~

這種寫法的好處是可以減少 `catch` 區塊內的巢狀條件式，而且程式碼也更簡潔。

例外篩選條件裡面也可以包含方法呼叫，例如：

~~~~~~~~
try
{
    // 執行資料異動。
}
catch (SqlException ex) when (HandleSqlError(ex) == false)
{
    // Log and throw.
}
...

bool HandleSqlError(SqlException ex)
{
    // 若已經處理好這個錯誤，就傳回 true，否則傳回 false。
}
~~~~~~~~

> 範例原始碼：[DemoExceptionFilter](examples/DemoExceptionFilter)。

## `catch` 和 `finally` 區塊中的 `await`

在處理例外狀況時，一種很常見的寫法是在 `catch` 或 `finally` 區塊中把錯誤訊息保存至某個記錄檔。由於寫入記錄檔是個 I/O 操作，所以使用非同步呼叫也是很合理的。於是，我們可能會想要這樣寫：

~~~~~~~~
try
{
    ...
}
catch (Exception ex)
{
    await LogAsync(ex.Tostring()); // C# 6 OK。C# 5 編譯失敗!
}
~~~~~~~~

若使用 C# 5 或更早版本的編譯器，倒數第二行會出現編譯錯誤："Cannot await a catch clause."，也就是 `catch` 子句中不可使用 `await`。C# 6 則解除了這個限制，讓 `catch` 和 `finally` 區塊裡面都可以寫 `await` 陳述式。

底下是一個比較完整的範例：

~~~~~~~~
using System;
using System.IO;
using System.Threading.Tasks;
...

static async Task<string> HttpGetStrAsync(string url)
{
    var client = new System.Net.Http.HttpClient();
    var streamTask = client.GetStringAsync(url);
    try
    {
        var responseText = await streamTask;
        return responseText;
    }
    catch (System.Net.Http.HttpRequestException ex)
    {
        await LogAsync($"失敗: {ex.Message}");
        return ex.Message;
    }
    finally
    {
        client.Dispose();
        await LogAsync("離開 HttpGHetAsync() 方法。");
    }
}

static async Task LogAsync(string s)
{
    await File.AppendAllTextAsync(@"c:\temp\log.txt", s);
}
~~~~~~~~

順便提及，實務上，用來保存記錄的 `LogAsync` 函式不應該再拋出例外，否則外層的 `catch` 區塊又會再次拋出新的例外，那麼第 18 行用來返回錯誤訊息的陳述式就執行不到了；也就是說，原本要記錄下來的例外會被丟棄，若將來要調閱記錄檔案來查問題，會找不到相關線索。

> 範例原始碼：[DemoExceptionAwait](examples/DemoExceptionAwait)。
