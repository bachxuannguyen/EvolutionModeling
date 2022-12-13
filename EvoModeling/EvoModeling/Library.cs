using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace EvoModeling
{
    public class Library
    {
        //CHUNG --- PRIVATE.

        //Biến ngẫu nhiên.
        private readonly Random r = new Random();
        //Hệ số dao động quanh giá trị chuẩn.
        private readonly int delta = 80;
        //Ngẫu nhiên hóa từ giá trị chuẩn.
        public int Randomize(int x, int y, int min, int max)
        {
            //x: giá trị chuẩn, y: độ biến thiên tính theo % của x, min: trả về tối thiểu, max: trả về tối đa.
            if (y < 0 || y > 100)
                return x;
            else
            {
                int t1 = x - y * x / 100;
                int t2 = x + y * x / 100;
                int z = r.Next(t1, t2 + 1);
                if (z < min)
                    return min;
                else if (z > max)
                    return max;
                else
                    return z;
            }
        }
        //Tính xác suất xảy ra biến cố dựa trên độ sâu.
        private double GetPByDepth(int d)
        {
            //Dùng hàm mũ với alpha nhỏ hơn 1.
            double alpha = 0.25;
            return Math.Pow(alpha, d);
        }

        //CHUNG --- PUBLIC.

        //Đường dẫn lưu file.
        public string folderPath = "D:/KHMT/Vs/EvoModeling/Png/";
        //Số thế hệ tối đa.
        public int maxGen = 50;
        //Gán mã định danh.
        public string getId_Random(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[r.Next(s.Length)]).ToArray());
        }        
        //Trọng số.
        public int wP = 0;
        public int wC = 2;
        public int wA = 1;
        //Tạo Id cho đối tượng.
        public int GetId_NewObject()
        {
            return 0;
        }
        
        //PHÂN HÓA.

        //Số lượng nút thực thi phân hóa.
        public int GetCount_DisNode(int totalNode)
        {
            //Trả về căn bậc 2 của tổng số nút.
            double alpha = 1.0;
            return Randomize(Convert.ToInt32(alpha * Math.Sqrt(totalNode)), delta, 1, totalNode);
        }
        //Số lượng nút được tạo mới khi phân hóa.
        public int GetCount_NewDisNode()
        {
            //Trả về từ a tới b.
            int a = 1;
            int b = 5;
            return r.Next(a, b + 1);
        }
        //Tỷ lệ nút mới tạo ra sẽ thực thi việc thay đổi.
        public double GetRatio_NewNodeUpdated = 0.5;

        //ĐỘT BIẾN.

        //Số lượng nút thực thi đột biến.
        public int GetCount_MutNode(int totalNode)
        {
            //Trả về căn bậc 2 của tổng số nút.
            double alpha = 0.5;
            return Randomize(Convert.ToInt32(alpha * Math.Sqrt(totalNode)), delta, 0, totalNode);
        }
        //Đột biến với một nút là mất hay thay đổi.
        public int GetType_Mutation(int totalNode)
        {
            //Không mất nút khi tổng số nút không lớn hơn a.
            //Ngoài ra b% số lần đột biến là mất nút.
            int a = 10;
            if (totalNode <= a)
                return 2;
            else
            {
                int b = 20;
                if (r.Next(1, 101) < b)
                    return 1;
                else
                    return 2;
            }
        }


        //NÚT BIẾN ĐỔI --- BIẾN ĐỔI LIÊN KẾT KẾ CẬN.

        //Xác xuất tăng, giảm hay vừa tăng vừa giảm số lượng nút kế cận khi một nút biến đổi.
        public int GetType_U_AdjNodeUpdate()
        {
            //a% là tăng.
            //b% là giảm.
            //Còn lại là vừa tăng vừa giảm.
            int a = 40;
            int b = 40;
            int x = r.Next(1, 101);
            if (x < a)
                return 1;
            else if (x < a + b)
                return 2;
            else
                return 3;
        }
        //Số lượng nút kế cận tăng / giảm khi một nút biến đổi.
        public int GetCount_U_AdjNodeUpdate_Add(int totalNode)
        {
            //Trả về căn bậc 2 của tổng số nút.
            double alpha = 0.75;
            return Randomize(Convert.ToInt32(alpha * Math.Sqrt(totalNode)), delta, 0, totalNode);
        }
        public int GetCount_U_AdjNodeUpdate_Minus(int totalANode)
        {
            //Trả về căn bậc 2 của tổng số nút kế cận.
            double alpha = 1.5;
            return Randomize(Convert.ToInt32(alpha * Math.Sqrt(totalANode)), delta, 0, totalANode);
        }

        //NÚT BIẾN ĐỔI --- LAN TRUYỀN TỚI NÚT CON.

        //Số lượng nút con biến đổi khi một nút biến đổi.
        public int GetCount_U_CNodeUpdate(int totalCNode, int d)
        {
            //Trả về a% của tổng số nút con.
            //Có xét ảnh hưởng của độ sâu lan truyền.
            int a = 50;
            return Convert.ToInt32(GetPByDepth(d) * Randomize(Convert.ToInt32(a * totalCNode / 100), delta, 0, totalCNode));
        }
        //Số lượng nút con biến mất khi một nút biến đổi.
        public int GetCount_U_CNodeDelete(int totalCNode, int d)
        {
            //Trả về a% của tổng số nút con.
            //Có xét ảnh hưởng của độ sâu lan truyền.
            int a = 3;
            return Convert.ToInt32(GetPByDepth(d) * Randomize(Convert.ToInt32(a * totalCNode / 100), delta, 0, totalCNode));
        }

        //NÚT BIẾN ĐỔI --- LAN TRUYỀN TỚI NÚT KẾ CẬN.

        //Số lượng nút kế cận biến đổi khi một nút biến đổi.
        public int GetCount_U_ANodeUpdate(int totalANode, int d)
        {
            //Trả về a% của tổng số nút kế cận.
            //Có xét ảnh hưởng của độ sâu lan truyền.
            int a = 20;
            return Convert.ToInt32(GetPByDepth(d) * Randomize(Convert.ToInt32(a * totalANode / 100), delta, 0, totalANode));
        }
        //Số lượng nút kế cận biến mất khi một nút biến đổi.
        public int getCount_U_ANodeDelete(int totalANode, int d)
        {
            //Trả về a% của tổng số nút kế cận.
            //Có xét ảnh hưởng của độ sâu lan truyền.
            int a = 3;
            return Convert.ToInt32(GetPByDepth(d) * Randomize(Convert.ToInt32(a * totalANode / 100), delta, 0, totalANode));
        }

        //NÚT BIẾN MẤT --- LAN TRUYỀN TỚI NÚT CON.

        //Nút cha biến mất thì toàn bộ nút con  biến mất.
        //Do đó không xét hai phương thức sau đây.
        //Số lượng nút con biến đổi khi một nút biến mất.
        //Số lượng nút con biến mất khi một nút biến mất.

        //NÚT BIẾN MẤT --- LAN TRUYỀN TỚI NÚT KẾ CẬN.

        //Số nút kế cận biến đổi khi một nút biến mất.
        public int getCount_D_ANodeUpdate(int totalANode, int d)
        {
            //Trả về a% của tổng số nút kế cận.
            //Có xét ảnh hưởng của độ sâu lan truyền.
            int a = 30;
            return Convert.ToInt32(GetPByDepth(d) * Randomize(Convert.ToInt32(a * totalANode / 100), delta, 0, totalANode));
        }
        //Số nút kế cận biến mất khi một nút biến mất.
        public int getCount_D_ANodeDelete(int totalANode, int d)
        {
            //Trả về a% của tổng số nút kế cận.
            //Có xét ảnh hưởng của độ sâu lan truyền.
            int a = 3;
            return Convert.ToInt32(GetPByDepth(d) * Randomize(Convert.ToInt32(a * totalANode / 100), delta, 0, totalANode));
        }

        //ĐÁNH GIÁ.

        //Ngưỡng số lượng nút để bắt đầu thực hiện các đánh giá.
        public int threshold_MeanNodeCount = 50;

        //Ngưỡng số nút chết, tỷ lệ so với số nút của thế hệ trước.
        public double threshold_DeadRatio = 0.1;

        //Ngưỡng cảnh báo số lần nút biến động, tỷ lệ so với tổng số nút sống của thế hệ trước.
        public double threshold_EventRatio = 10.0;

        //Ngưỡng số lần chết để thực hiện phân tích.
        public int threshold_DeathCount = 3;
        public int threshold_DeathCount_LetItGo = 5;

    }
}
