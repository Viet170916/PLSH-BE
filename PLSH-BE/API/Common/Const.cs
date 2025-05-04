namespace API.Common;

public class Const
{
  public const string CLIENT_HOST = "https://book-hive.space";
  public const string LIB_CLIENT_HOST = "https://librarian.book-hive.space";

  public const string GEN_BOOK_FIELS_PROMT = @"
yêu cầu hãy tạo cho tôi 1 dữ liệu chuỗi dạng json có cấu trúc như này type Book = {
  id: number;
  title: string;
  description: string;
  language: string;
  pageCount: number;
  isbnNumber10: string;
  isbnNumber13: string;
  otherIdentifier: string | null;
  publishDate: string;
  publisher: string;
  version: string;
  category: {
    name: string;
    description: string | null;
  };
  newCategory: {
    name: string;
    description: string | null;
    chosen: boolean;
  };
  authors: {
    fullName: string;
    avatarUrl: string | null;
    description: string | null;
    birthYear: number | null;
    deathYear: number | null;
  }[];
thumbnail: string;
}, khi mà người dùng nhập promt yêu cầu làm một việc gì đấy thì hãy tạo teo yêu cầu của người dùng và đưa dữ liệu đó vào
type Book,
sau đó chuyển dữ liệu này thành kiểu trả ra 1 dữ liệu chuỗi dạng json có cấu trúc như này {
mesage:string,
data:{
  id: number;
  title: string;
  description: string;
  language: string;
  pageCount: number;
  isbnNumber10: string;
  isbnNumber13: string;
  otherIdentifier: string | null;
  publishDate: string;
  publisher: string;
  version: string;
  category: {
    name: string;
    description: string | null;
  };
  newCategory: {
    name: string;
    description: string | null;
    chosen: boolean;
  };
  authors: {
    fullName: string;
    avatarUrl: string | null;
    description: string | null;
    birthYear: number | null;
    deathYear: number | null;
  }[];
thumbnail: string;
}}, 
 value là giá trị mà bạn được người dùng yêu cầu phân tích, 
nếu có yêu cầu về tác giả hãy tìm thông tin chính xác và trả vào trường authors với mảng các tác giả  {
    fullName: string;
    avatarUrl: string | null;
    description: string | null;
    birthYear: number | null;
    deathYear: number | null;
  }[],
nếu có yêu cầu về thể loại hoặc bạn muốn gen ra thể loại khi tìm được dữ liệu hợp lệ thì hãy để nó trong trường newCategory với chosen: true và không có id,
 message là lời phản hồi của chat bot với người dùng, cố gắng đưa ra lời nói thân thiện 1 chút, không quá cứng nhắc, các dữ liệu trả ra sẽ được tự động sử dụng ở chỗ khác chứ không in ra trực tiếp tại chat box người dùng nên message không cần phải nói đại khái như ' sau đây là dữ liệu' nó nghe quá cứng nhắc, thay vào đó có thể nói dữ liệu đã được tớ xử lý xong, bạn hãy kiểm tra kỹ lại nhé bởi tớ vẫn không thể chắc chắn thông tin tớ cung cấp là đúng 100% đâu nè! đây chỉ là 1 câu message ví dụ chứ không phải lúc nào cũng dùng câu này, hãy paraphase lại câu này cho mỗi lần gen khác nhau, ví dụ có cái gì đó mà chat bot không làm được có thể thông báo cho người dùng qua đây để người dùng lưu ý, hoặc vân vân tuỳ vào ngữ cảnh người dùng hỏi, message không đưa ra các thông báo quá kỹ thuật khiến người dùng không hiểu về code sẽ không hiểu bạn đang nói gì, ví dụ dữ liệu là null chỉ cần nó là trường đó không tìm được kết quả chính xác thôi chẳng hạn,
 dữ liệu bắt buộc phải chính xác, nếu dữ liệu không tìm thấy thì để null chứ đừng có cố bịa ra, có thể tra dữ liệu ở đâu đó, nhất là các dữ liệu hình ảnh không được để thông tin là example hay là gì, không biết thì cứ để null, dữ liệu trả về chỉ trả đúng chuỗi json không cần thêm bất kì lời thoại không liên quan nào vì đây là kết quả phục vụ cho ứng dụng của tôi, bắt buộc không được trả ra dữ liệu thừa thãi, ngôn ngữ để dạng mã cho tôi ví dụ như vi hay en, các id để null cho tôi, dữ liệu cố gắng đưa ra chi tiết chút, nhất là mô tả, sử dụng chủ đạo là ngôn ngữ tiếng việt trừ tên sách và tên tác giả hãy để ngôn ngữ gốc. 
 dữ liệu nào bị null thì bỏ qua không đưa vào json,
có bất cứ yêu cầu nào không liên quan đến xử lý dự liệu của sách thì đều không xử lý và hãy trả về message ""Yêu cầu của bạn không phù hợp với vai trò của tôi tại đây, tôi không thể giúp bạn việc này được!"", 
nên nhớ authors là 1 mảng các author nên lúc nào dù chỉ có 1 tác giả cũng phải trả về mảng
sau thuộc tính của cùng của object hay mảng không thêm dấu phẩy thừa. luôn nhớ phải trả ra đúng định dạng json như đã yêu cầu, sau đây là yêu cầu của người dùng: 
""với cuốn sách ""{{bookName}}"" hãy thực hiện yêu cầu sau đây, {{prompt}}""

";
  // và chỉ có thể để là number hoặc string hoặc boolean hoặc null, không được để là object hay mảng, nếu thuộc tính là object hay mảng thì hãy để key là giá trị nested đến các thuộc tính con đó,
  // với key là key của type Book với việc là chuỗi key này có thể nest sâu vào cả trong các thuộc tính là object hay mảng,

  public const string ANALIZE_PHASE_OF_BOOK_PROMT = @"Hãy chia đoạn cho tôi với đoạn text là nội dung quyển sách này, 
yêu cầu chia đoạn là mỗi đoạn khoảng 200 đến 300 từ, lấy làm sao để không bị ngắt đoạn mạch ý nghĩa giữa các câu trong đoạn, loại bỏ hết ký tự thừa thãi nếu có, chỉ để lại các ký tự có ý nghĩa, chỉ lấy văn bản là nội dung của cuốn sách, bỏ hết các nội dung không phải là nội dung quyển sách ví dụ như title, mục lục, các lời giới thiệu sách hay giới thiệu tác giả, ...,

nếu text trả về không phải nội dung sách thì trả ra mảng rỗng []
trong nội dung nhận được là text lấy từ epub nên có thể chứa các text thừa như số trang hoặc tên chapter thì hãy bỏ luôn các text đó, 
dữ liệu trả về là 1 json hợp lệ là mảng {text: string, p: number}[] với text là nội dung đoạn, p là thứ tự vị trí đoạn, môi đoạn tạo ra 1 phần tử mảng rồi add vào mảng này, và trả ra kết quả cuối cùng là chuỗi json, đây là 1 chức năng phần mềm nên không được thêm bất cứ mesage hay text nào không liên quan đến kết quả trả ra làm ảnh hưởng đến việc parse json, không để dấu phẩy thừa ở các thuộc tính cuối cùng của object hay phần tử cuối của mảng tránh .net parse lỗi.
sau đây là phần text nội dung chapter sách cần chia đoạn: ""{{text}}""
";

  public const string TODAY_QUOTE_PROMT =
    "hãy trích dẫn 1 câu nói nổi tiếng của một nhà văn bất kỳ, trả về dạng json {title: string, content:string, author: string}, đây là 1 chức năng hiển thị trên phần giới thiệu của app, không trả lời bất kỳ lời nhắn thừa, chỉ trả ra đúng json như yêu cầu, không để thừa dấu phẩy cuối của thuộc tính cuối";
}
