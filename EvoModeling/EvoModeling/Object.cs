using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoModeling
{
    public class Object : ICloneable
    {
        private Random r = new Random();
        private Library lib = new Library();
        public Node[] node;
        public double tracer_disRatio;
        public double tracer_mutRatio;

        public object Clone()
        {
            Object clonedObj = new Object();
            Array.Resize(ref clonedObj.node, node.Length);
            if (node.Length > 0)
            {
                for (int i = 0; i < node.Length; i++)
                {
                    clonedObj.node[i] = new Node();
                    clonedObj.node[i] = node[i].Clone() as Node;
                }
            }
            clonedObj.tracer_disRatio = tracer_disRatio;
            clonedObj.tracer_mutRatio = tracer_mutRatio;
            return clonedObj;
        }

        public bool Init()
        {
            node = new Node[] { };
            tracer_disRatio = 0.0;
            tracer_mutRatio = 0.0;

            return true;
        }

        public EventLog[] nodeCreate(bool eventSource, bool threadInit, int[] pNodeId, int gen)
        {            
            string eventId = lib.getId_Random(5);

            EventLog[] currLog = new EventLog[1];
            currLog[0] = new EventLog();
            EventLog[] accLog = new EventLog[] { };
            
            //1. TẠO NÚT MỚI.

            Array.Resize(ref node, node.Length + 1);
            int newId = node.Length - 1;
            node[newId] = new Node();
            node[newId].id = newId;
            
            node[newId].alive = true;
            node[newId].pNode = pNodeId;
            node[newId].aNode = new int[] { };
            node[newId].cNode = new int[] { };

            ////
            currLog[0].id = eventId;
            currLog[0].eventSource = eventSource;
            currLog[0].threadInit = threadInit;
            currLog[0].nodeId = newId;
            ////

            //2. LIÊN KẾT TỚI VÀI NÚT KẾ CẬN.

            double d = (double)r.Next(100) / 100;
            if (d < lib.GetRatio_NewNodeUpdated)
            {
                int[] exception = { newId };
                accLog = nodeUpdate(eventSource, true, newId, 0, exception, gen);
            }

            ////
            if (accLog.Length > 0)
                currLog = updateCurrLog(currLog, accLog, 3);
            ////

            //Không nhất thiết phải ghi lại sự kiện Create().
            currLog = new EventLog[] { };
            currLog = currLog.Concat(accLog).ToArray();

            //3. ĐỒNG BỘ LIÊN KẾT CHO NÚT CHA.

            int[] myPNode = node[newId].pNode;
            if (myPNode.Length > 0)
                myPNode = myPNode.Where(x => node[x].alive).ToArray();
            if (myPNode.Length > 0)
            {
                for (int i = 0; i < myPNode.Length; i++)
                {
                    int[] y = new int[] { node[newId].id };
                    node[myPNode[i]].cNode = node[myPNode[i]].cNode.Concat(y).ToArray().Distinct().ToArray();
                }
            }

            //4. ĐỒNG BỘ LIÊN KẾT CHO NÚT CON.
            //Nút tạo mới không có nút con. Đoạn mã sau đây thường sẽ không được thực thi.

            int[] myCNode = node[newId].cNode;
            if (myCNode.Length > 0)
                myCNode = myCNode.Where(x => node[x].alive).ToArray();
            if (myCNode.Length > 0)
            {
                for (int i = 0; i < myCNode.Length; i++)
                {
                    int[] y = new int[] { node[newId].id };
                    node[myCNode[i]].pNode = node[myCNode[i]].pNode.Concat(y).ToArray().Distinct().ToArray();
                }
            }

            //5. ĐỒNG BỘ LIÊN KẾT CHO NÚT KẾ CẬN.

            int[] myANode = node[newId].aNode;
            if (myANode.Length > 0)
                myANode = myANode.Where(x => node[x].alive).ToArray();
            if (myANode.Length > 0)
            {
                for (int i = 0; i < myANode.Length; i++)
                {
                    int[] y = new int[] { node[newId].id };
                    node[myANode[i]].aNode = node[myANode[i]].aNode.Concat(y).ToArray().Distinct().ToArray();
                }
            }

            return currLog;
        }

        public EventLog[] nodeDelete(bool eventSource, bool threadInit, int inputNodeId, int depth, int[] exception, int gen)
        {
            string eventId = lib.getId_Random(5);

            if (node[inputNodeId].alive)
            {
                ////
                EventLog[] currLog = new EventLog[1];
                currLog[0] = new EventLog();
                currLog[0].id = eventId;
                currLog[0].eventSource = eventSource;
                currLog[0].threadInit = threadInit;
                currLog[0].nodeId = inputNodeId;
                currLog[0].count_Delete = 1;
                currLog[0].count_Total = 1;
                currLog[0].count_AccDelete = 1;
                currLog[0].count_AccTotal = 1;
                EventLog[] accLog = new EventLog[] { };
                ////

                string depthStr = "";
                for (int i = 0; i < depth + 1; i++)
                    depthStr += ".";
                //Console.WriteLine(eventId + " Del " + depthStr + "[" + inputNodeId.ToString() + "." + node[inputNodeId].pNode[0].ToString() +"," + node[inputNodeId].cNode.Length.ToString() + "," + node[inputNodeId].aNode.Length.ToString() + "]");
                Console.Write("*" + gen.ToString() + "*");

                //1. LAN TRUYỀN XÓA VÀI NÚT KẾ CẬN.

                int[] myANode = node[inputNodeId].aNode;
                if (myANode.Length > 0)
                    myANode = myANode.Where(x => node[x].alive).ToArray();
                for (int i = 0; i < exception.Length; i++)
                    myANode = myANode.Where(x => x != exception[i]).ToArray();
                if (myANode.Length > 0)
                {
                    int dCount = lib.getCount_D_ANodeDelete(myANode.Length, depth + 1);
                    int[] dNode = myANode.OrderBy(x => r.Next()).Take(dCount).ToArray();
                    if (dNode.Length > 0)
                    {
                        int[] newException = { inputNodeId };
                        newException = newException.Concat(exception).ToArray().Distinct().ToArray();
                        for (int i = 0; i < dNode.Length; i++)
                        {
                            accLog = nodeDelete(eventSource, false, dNode[i], depth + 1, newException, gen).Concat(accLog).ToArray();
                            currLog = updateCurrLog(currLog, accLog, 1);
                        }
                    }
                }

                //2. LAN TRUYỀN BIẾN ĐỔI VÀI NÚT KẾ CẬN.

                myANode = node[inputNodeId].aNode;
                if (myANode.Length > 0)
                    myANode = myANode.Where(x => node[x].alive).ToArray();
                for (int i = 0; i < exception.Length; i++)
                    myANode = myANode.Where(x => x != exception[i]).ToArray();
                if (myANode.Length > 0)
                {
                    int uCount = lib.getCount_D_ANodeUpdate(myANode.Length, depth + 1);
                    int[] uNode = myANode.OrderBy(x => r.Next()).Take(uCount).ToArray();
                    if (uNode.Length > 0)
                    {
                        int[] newException = { inputNodeId };
                        newException = newException.Concat(exception).ToArray().Distinct().ToArray();
                        for (int i = 0; i < uNode.Length; i++)
                        {
                            accLog = nodeUpdate(eventSource, false, uNode[i], depth + 1, newException, gen).Concat(accLog).ToArray();
                            ////
                            if (accLog.Length > 0)
                                currLog = updateCurrLog(currLog, accLog, 2);
                            ////
                        }
                    }
                }

                //3. ĐÁNH DẤU NÚT ĐÃ CHẾT.

                node[inputNodeId].alive = false;

                //4. ĐỒNG BỘ LIÊN KẾT CHO NÚT KẾ CẬN.

                myANode = node[inputNodeId].aNode;
                if (myANode.Length > 0)
                    myANode = myANode.Where(x => node[x].alive).ToArray();
                if (myANode.Length > 0)
                {
                    for (int i = 0; i < myANode.Length; i++)
                    {
                        if (node[myANode[i]].aNode.Contains(inputNodeId))
                        {
                            node[myANode[i]].aNode = node[myANode[i]].aNode.Where(x => x != inputNodeId).ToArray();
                        }
                    }
                }

                //5. ĐỒNG BỘ LIÊN KẾT CHO NÚT CHA.

                int[] myPNode = node[inputNodeId].pNode;
                if (myPNode.Length > 0)
                    myPNode = myPNode.Where(x => node[x].alive).ToArray();
                if (myPNode.Length > 0)
                {
                    for (int i = 0; i < myPNode.Length; i++)
                    {
                        if (node[myPNode[i]].cNode.Contains(inputNodeId))
                        {
                            node[myPNode[i]].cNode = node[myPNode[i]].cNode.Where(x => x != inputNodeId).ToArray();
                        }
                    }
                }
                
                //6. XÓA CÁC NÚT CON.

                int[] myCNode = node[inputNodeId].cNode;
                if (myCNode.Length > 0)
                    myCNode = myCNode.Where(x => node[x].alive).ToArray();
                for (int i = 0; i < exception.Length; i++)
                    myCNode = myCNode.Where(x => x != exception[i]).ToArray();
                if (myCNode.Length > 0)
                {
                    int[] newException = { inputNodeId };
                    newException = newException.Concat(exception).ToArray().Distinct().ToArray();
                    for (int i = 0; i < myCNode.Length; i++)
                    {
                        accLog = nodeDelete(eventSource, false, myCNode[i], depth + 1, newException,gen).Concat(accLog).ToArray();
                        ////
                        if (accLog.Length > 0)
                            currLog = updateCurrLog(currLog, accLog, 1);
                        ////
                    }
                }

                currLog = currLog.Concat(accLog).ToArray();

                return currLog;
            }
            else
            {
                EventLog[] currLog = new EventLog[] { };
                return currLog;
            }
        }

        public EventLog[] nodeUpdate(bool eventSource,bool threadInit, int inputNodeId, int depth, int[] exception, int gen)
        {
            string eventId = lib.getId_Random(5);

            if (node[inputNodeId].alive)
            {
                ////
                EventLog[] currLog = new EventLog[1];
                currLog[0] = new EventLog();
                currLog[0].id = eventId;
                currLog[0].eventSource = eventSource;
                currLog[0].threadInit = threadInit;
                currLog[0].nodeId = inputNodeId;
                currLog[0].count_Update = 1;
                currLog[0].count_Total = 1;
                currLog[0].count_AccUpdate = 1;
                currLog[0].count_AccTotal = 1;
                EventLog[] accLog = new EventLog[] { };
                ////

                //1. BIẾN ĐỔI DANH SÁCH NÚT KẾ CẬN.
                //Bao gồm việc đồng bộ liên kết cho nút kế cận.

                int[] myANode = node[inputNodeId].aNode;
                if (myANode.Length > 0)
                    myANode = myANode.Where(x => node[x].alive).ToArray();
                node[inputNodeId].aNode = aNodeUpdate(eventId, inputNodeId, myANode, depth, gen);

                //2. XÓA BỎ VÀI NÚT CON.

                int[] myCNode = node[inputNodeId].cNode;
                if (myCNode.Length > 0)
                    myCNode = myCNode.Where(x => node[x].alive).ToArray();
                for (int i = 0; i < exception.Length; i++)
                    myCNode = myCNode.Where(x => x != exception[i]).ToArray();
                if (myCNode.Length > 0)
                {
                    int dCount = lib.GetCount_U_CNodeDelete(myCNode.Length, depth + 1);
                    int[] dNode = myCNode.OrderBy(x => r.Next()).Take(dCount).ToArray();
                    if (dNode.Length > 0)
                    {
                        int[] newException = { inputNodeId };
                        newException = newException.Concat(exception).ToArray().Distinct().ToArray();
                        for (int i = 0; i < dNode.Length; i++)
                        {
                            accLog = nodeDelete(eventSource, false, dNode[i], depth + 1, newException, gen).Concat(accLog).ToArray();
                            currLog = updateCurrLog(currLog, accLog, 1);
                        }
                    }
                }

                //3. BIẾN ĐỔI VÀI NÚT CON.

                myCNode = node[inputNodeId].cNode;
                if (myCNode.Length > 0)
                    myCNode = myCNode.Where(x => node[x].alive).ToArray();
                for (int i = 0; i < exception.Length; i++)
                    myCNode = myCNode.Where(x => x != exception[i]).ToArray();
                if (myCNode.Length > 0)
                {
                    int uCount = lib.GetCount_U_CNodeUpdate(myCNode.Length, depth + 1);
                    int[] uNode = myCNode.OrderBy(x => r.Next()).Take(uCount).ToArray();
                    if (uNode.Length > 0)
                    {
                        int[] newException = { inputNodeId };
                        newException = newException.Concat(exception).ToArray().Distinct().ToArray();
                        for (int i = 0; i < uNode.Length; i++)
                        {
                            accLog = nodeUpdate(eventSource, false, uNode[i], depth + 1, newException, gen).Concat(accLog).ToArray();
                            currLog = updateCurrLog(currLog, accLog, 2);
                        }
                    }
                }

                //4. XÓA BỎ VÀI NÚT KẾ CẬN.

                myANode = node[inputNodeId].aNode;
                if (myANode.Length > 0)
                    myANode = myANode.Where(x => node[x].alive).ToArray();
                for (int i = 0; i < exception.Length; i++)
                    myANode = myANode.Where(x => x != exception[i]).ToArray();
                if (myANode.Length > 0)
                {
                    int dCount = lib.getCount_U_ANodeDelete(myANode.Length, depth + 1);
                    int[] dNode = myANode.OrderBy(x => r.Next()).Take(dCount).ToArray();
                    if (dNode.Length > 0)
                    {
                        int[] newException = { inputNodeId };
                        newException = newException.Concat(exception).ToArray().Distinct().ToArray();
                        for (int i = 0; i < dNode.Length; i++)
                        {
                            accLog = nodeDelete(eventSource, false, dNode[i], depth + 1, newException, gen).Concat(accLog).ToArray();
                            currLog = updateCurrLog(currLog, accLog, 1);
                        }
                    }
                }

                //5. BIẾN ĐỔI VÀI NÚT KẾ CẬN.

                myANode = node[inputNodeId].aNode;
                if (myANode.Length > 0)
                    myANode = myANode.Where(x => node[x].alive).ToArray();
                for (int i = 0; i < exception.Length; i++)
                    myANode = myANode.Where(x => x != exception[i]).ToArray();
                if (myANode.Length > 0)
                {
                    int uCount = lib.GetCount_U_ANodeUpdate(myANode.Length, depth + 1);
                    int[] uNode = myANode.OrderBy(x => r.Next()).Take(uCount).ToArray();
                    if (uNode.Length > 0)
                    {
                        int[] newException = { inputNodeId };
                        newException = newException.Concat(exception).ToArray().Distinct().ToArray();
                        for (int i = 0; i < uNode.Length; i++)
                        {
                            accLog = nodeUpdate(eventSource, false, uNode[i], depth + 1, newException, gen).Concat(accLog).ToArray();
                            currLog = updateCurrLog(currLog, accLog, 1);
                        }
                    }
                }

                currLog = currLog.Concat(accLog).ToArray();

                return currLog;
            }
            else
            {
                EventLog[] currLog = new EventLog[] { };
                return currLog;
            }
        }

        EventLog[] updateCurrLog(EventLog[] currLog, EventLog[] accLog, int eventCode)
        {
            EventLog[] returnLog = currLog;

            /*
            if (eventCode == 0)
                returnLog[0].count_Create += 1;
            else if (eventCode == 1)
                returnLog[0].count_Delete += 1;
            else if (eventCode == 2)
                returnLog[0].count_Update += 1;
            returnLog[0].count_Total = returnLog[0].count_Create + returnLog[0].count_Delete + returnLog[0].count_Update;
            */

            if (accLog.Length > 0)
            {
                Array.Resize(ref currLog[0].accLogEntry, currLog[0].accLogEntry.Length + 1);
                returnLog[0].accLogEntry[currLog[0].accLogEntry.Length - 1] = accLog[0].id;
                returnLog[0].count_AccCreate = accLog[0].count_AccCreate + returnLog[0].count_AccCreate;
                returnLog[0].count_AccDelete = accLog[0].count_AccDelete + returnLog[0].count_AccDelete;
                returnLog[0].count_AccUpdate = accLog[0].count_AccUpdate + returnLog[0].count_AccUpdate;
                returnLog[0].count_AccTotal = returnLog[0].count_AccCreate + returnLog[0].count_AccDelete + returnLog[0].count_AccUpdate;
            }

            return returnLog;

        }

        public int[] aNodeUpdate(string eventId, int inputNodeId, int[] inputANodeId, int depth, int gen)
        {
            //Số lượng nút kế cận sẽ biến thiên.
            int x = lib.GetType_U_AdjNodeUpdate();
            int y1 = lib.GetCount_U_AdjNodeUpdate_Add(node.Length);
            int y2 = lib.GetCount_U_AdjNodeUpdate_Minus(node[inputNodeId].aNode.Length);

            //Chuỗi đánh dấu độ sâu lan truyền.
            string depthStr = "";
            for (int i = 0; i < depth + 1; i++)
                depthStr += ".";

            switch (x)
            {
                //Tăng, giảm hay giữ nguyên.
                case 1:
                    {
                        //Thông báo ra.
                        //Console.Write(eventId + " Upd " + depthStr + "[" + inputNodeId.ToString() + "." + node[inputNodeId].pNode[0].ToString() + "," + node[inputNodeId].cNode.Length.ToString() + "," + node[inputNodeId].aNode.Length.ToString() + "] +" + y1.ToString());
                        string aNodeStr = "";

                        //Chọn ngẫu nhiên x nút sống từ đối tượng, cho phép trùng các nút hiện kế cận. 
                        int[] index = new int[node.Length];
                        for (int i = 0; i < index.Length; i++)
                            index[i] = i;
                        index = index.Where(z => node[z].alive).ToArray();
                        
                        //Đưa thêm nút vào mảng kế cận.
                        int[] myUANode = index.OrderBy(z => r.Next()).Take(y1).ToArray();

                        //Cập nhật chuỗi ghi các nút kế cận.
                        if (myUANode.Length > 0)
                        {
                            for (int j = 0; j < myUANode.Length; j++)
                            {
                                myUANode[j] = node[myUANode[j]].id;
                                //aNodeStr += "[+" + myUANode[j].ToString() + "]";
                            }
                        }

                        //Loại bỏ các nút đã chết.
                        if (myUANode.Length > 0)
                            myUANode = myUANode.Where(z => node[z].alive && z != inputNodeId).ToArray();

                        //Thực hiện việc cập nhật.
                        if (myUANode.Length > 0)
                        {
                            //Cập nhật tiếp chuỗi ghi các nút kế cận.
                            //aNodeStr += "*";
                            for (int j = 0; j < myUANode.Length; j++)
                                aNodeStr += "<+" + myUANode[j].ToString() + ">";

                            //Ghép mảng kế cận vào nút.
                            inputANodeId = inputANodeId.Concat(myUANode).ToArray().Distinct().ToArray();

                            //Cập nhật liên kết cho nút kế cận.
                            if (inputANodeId.Length > 0)
                            {
                                for (int i = 0; i < inputANodeId.Length; i++)
                                {
                                    int[] t = new int[] { node[inputNodeId].id };
                                    node[inputANodeId[i]].aNode = node[inputANodeId[i]].aNode.Concat(t).ToArray().Distinct().ToArray();
                                }
                            }
                        }

                        //Hoàn chỉnh thông báo.
                        //Console.Write(" " + aNodeStr + "\r\n");
                        Console.Write("-" + gen.ToString() + "-");

                        return inputANodeId;
                    }
                case 2:
                    {
                        //Xuất ra.
                        //Console.Write(eventId + " Upd " + depthStr + "[" + inputNodeId.ToString() + "." + node[inputNodeId].pNode[0].ToString() + "," + node[inputNodeId].cNode.Length.ToString() + "," + node[inputNodeId].aNode.Length.ToString() + "] -" + y2.ToString());
                        string aNodeStr = "";

                        //Loại bỏ ngẫu nhiên x nút hiện kế cận.
                        int[] index = new int[inputANodeId.Length];
                        for (int i = 0; i < index.Length; i++)
                            index[i] = i;
                        index = index.Where(z => node[z].alive).ToArray();
                        //Đưa thêm nút sẽ loại bỏ khỏi mảng kế cận.
                        int[] myUANode = index.OrderBy(z => r.Next()).Take(y2).ToArray();

                        //Cập nhật chuỗi ghi các nút kế cận.
                        if (myUANode.Length > 0)
                        {
                            for (int j = 0; j < myUANode.Length; j++)
                            {
                                myUANode[j] = inputANodeId[myUANode[j]];
                                //aNodeStr += "[-" + myUANode[j].ToString() + "]";
                            }
                        }

                        //Loại bỏ các nút đã chết.
                        if (myUANode.Length > 0)
                            myUANode = myUANode.Where(z => node[z].alive && z != inputNodeId).ToArray();

                        //Thực hiện việc cập nhật.
                        if (myUANode.Length > 0)
                        {
                            //Cập nhật tiếp chuỗi ghi các nút kế cận.
                            //aNodeStr += "*";
                            for (int j = 0; j < myUANode.Length; j++)
                                aNodeStr += "<-" + myUANode[j].ToString() + ">";

                            List<int> lst = inputANodeId.OfType<int>().ToList();
                            for (int i = 0; i < myUANode.Length; i++)
                            {
                                //Loại bỏ từng nút.
                                lst.RemoveAll(t => t == myUANode[i]);

                                //Cập nhật cho nút kế cận.
                                if (node[myUANode[i]].aNode.Contains(inputNodeId))
                                {
                                    node[myUANode[i]].aNode = node[myUANode[i]].aNode.Where(z => z != inputNodeId).ToArray();
                                }
                            }
                            inputANodeId = lst.ToArray();
                        }

                        //Hoàn chỉnh thông báo.
                        //Console.Write(" " + aNodeStr + "\r\n");
                        Console.Write("-" + gen.ToString() + "-");

                        return inputANodeId;
                    }
                case 3:
                    {
                        //Thông báo ra.
                        //Console.Write(eventId + " Upd " + depthStr + "[" + inputNodeId.ToString() + "." + node[inputNodeId].pNode[0].ToString() + "," + node[inputNodeId].cNode.Length.ToString() + "," + node[inputNodeId].aNode.Length.ToString() + "] +" + y1.ToString() + " -" + y2.ToString());
                        string aNodeStr = "";

                        //Thêm trước.
                        //Chọn ngẫu nhiên x nút sống từ đối tượng, cho phép trùng các nút hiện kế cận. 
                        int[] index = new int[node.Length];
                        for (int i = 0; i < index.Length; i++)
                            index[i] = i;
                        index = index.Where(z => node[z].alive).ToArray();

                        //Đưa thêm nút vào mảng kế cận.
                        int[] myUANode = index.OrderBy(z => r.Next()).Take(y1).ToArray();

                        //Cập nhật chuỗi ghi các nút kế cận.
                        if (myUANode.Length > 0)
                        {
                            for (int j = 0; j < myUANode.Length; j++)
                            {
                                myUANode[j] = node[myUANode[j]].id;
                                //aNodeStr += "[+" + myUANode[j].ToString() + "]";
                            }
                        }

                        //Loại bỏ các nút đã chết.
                        if (myUANode.Length > 0)
                            myUANode = myUANode.Where(z => node[z].alive && z != inputNodeId).ToArray();

                        //Thực hiện việc cập nhật.
                        if (myUANode.Length > 0)
                        {
                            //aNodeStr += "*";
                            //Cập nhật chuỗi ghi các nút kế cận.
                            for (int j = 0; j < myUANode.Length; j++)
                                aNodeStr += "<+" + myUANode[j].ToString() + ">";

                            //Ghép mảng kế cận vào nút.
                            inputANodeId = inputANodeId.Concat(myUANode).ToArray().Distinct().ToArray();

                            //Cập nhật liên kết cho nút kế cận.
                            if (inputANodeId.Length > 0)
                            {
                                for (int i = 0; i < inputANodeId.Length; i++)
                                {
                                    int[] t = new int[] { node[inputNodeId].id };
                                    node[inputANodeId[i]].aNode = node[inputANodeId[i]].aNode.Concat(t).ToArray().Distinct().ToArray();
                                }
                            }
                        }

                        //Bớt sau.
                        aNodeStr += " ";
                        //Sao lưu mảng nút kế cận hiện tại.
                        
                        //Loại bỏ ngẫu nhiên x nút hiện kế cận.
                        index = new int[inputANodeId.Length];
                        for (int i = 0; i < index.Length; i++)
                            index[i] = i;

                        //Đưa thêm nút sẽ loại bỏ khỏi mảng kế cận.
                        myUANode = index.OrderBy(z => r.Next()).Take(y2).ToArray();

                        //Cập nhật chuỗi ghi các nút kế cận.
                        if (myUANode.Length > 0)
                        {
                            for (int j = 0; j < myUANode.Length; j++)
                            {
                                myUANode[j] = inputANodeId[myUANode[j]];
                                //aNodeStr += "[-" + myUANode[j].ToString() + "]";
                            }
                        }

                        //Loại bỏ các nút đã chết.
                        if (myUANode.Length > 0)
                            myUANode = myUANode.Where(z => node[z].alive && z != inputNodeId).ToArray();

                        //Thực hiện việc cập nhật.
                        if (myUANode.Length > 0)
                        {
                            //Cập nhật tiếp chuỗi ghi các nút kế cận.
                            //aNodeStr += "*";
                            for (int j = 0; j < myUANode.Length; j++)
                                aNodeStr += "<-" + myUANode[j].ToString() + ">";

                            List<int> lst = inputANodeId.OfType<int>().ToList();
                            for (int i = 0; i < myUANode.Length; i++)
                            {
                                //Loại bỏ từng nút.
                                lst.RemoveAll(t => t == myUANode[i]);

                                //Cập nhật cho nút kế cận.
                                if (node[myUANode[i]].aNode.Contains(inputNodeId))
                                {
                                    node[myUANode[i]].aNode = node[myUANode[i]].aNode.Where(z => z != inputNodeId).ToArray();
                                }
                            }
                            inputANodeId = lst.ToArray();
                        }

                        //Hoàn chỉnh thông báo.
                        //Console.Write(" " + aNodeStr + "\r\n");
                        Console.Write("-" + gen.ToString() + "-");

                        return inputANodeId;
                    }
            }

            return inputANodeId;
        }

        public bool weightCalculate(int inputNodeId)
        {
            //Lấy số liên kết cha, con và đồng cấp.
            int p = node[inputNodeId].pNode.Length;
            int c = node[inputNodeId].cNode.Length;
            int a = node[inputNodeId].aNode.Length;

            //Lấy trọng số của từng loại liên kết.
            int wP = lib.wP;
            int wC = lib.wC;
            int wA = lib.wA;

            //Tính trọng số của nút dựa trên các liên kết.
            node[inputNodeId].weight = p * wP + c * wC + a * wA;

            return true;
        }

        public Node[] getAliveNodes()
        {
            //Tạo mới mảng các nút.
            Node[] aliveNode = new Node[] { };

            //Duyệt đưa các nút còn sống vào mảng mới.
            if (node.Length > 0)
            {
                for (int i = 0; i < node.Length; i++)
                {
                    if (node[i].alive)
                    {
                        Array.Resize(ref aliveNode, aliveNode.Length + 1);
                        aliveNode[aliveNode.Length - 1] = node[i];
                    }
                }
            }

            return aliveNode;
        }

    }
}
