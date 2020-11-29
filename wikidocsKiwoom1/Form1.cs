﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace wikidocsKiwoom1
{
    public partial class Form1 : Form
    {
        List<StockInfo> stockList;
        List<AutoTradingRule> autoTradingRuleList;
        List<stockBalance> stockBalanceList;
        List<outstanding> outstandingList;

        int autoSreenNumber = 1000;
        int autoRuleID = 0;
        string currentCondition = "";
        public static string ACCOUNT_NUMBER = "";

        public Form1()
        {
            InitializeComponent();

            axKHOpenAPI1.OnReceiveRealData += onReceiveRealData;
            axKHOpenAPI1.OnReceiveTrData += onReceiveTrData;
            axKHOpenAPI1.OnEventConnect += onEventConnect;
            axKHOpenAPI1.OnReceiveTrCondition += onReceiveTrCondition;
            axKHOpenAPI1.OnReceiveRealCondition += onReceiveRealCondition;
            axKHOpenAPI1.OnReceiveConditionVer += onReceiveConditionVer;
            axKHOpenAPI1.OnReceiveChejanData += onReceiveChejanData;

            setAutoTradingRuleButton.Click += ButtonClicked;
            autoTradingStartButton.Click += ButtonClicked;
            autoTradingStopButton.Click += ButtonClicked;
            SellAllStockButton.Click += ButtonClicked;

            stockSearchButton.Click += ButtonClicked;
            balanceCheckButton.Click += ButtonClicked;

            buyButton.Click += ButtonClicked;
            sellButton.Click += ButtonClicked;

            orderFixButton.Click += ButtonClicked;
            orderCancelButton.Click += ButtonClicked;

            limitPriceNumericUpDown.ValueChanged += setBuyingPerStock;
            limitNumberNumericUpDown.ValueChanged += setBuyingPerStock;

            balanceDataGridView.SelectionChanged += dataGridViewSelectionChanged;
            outstandingDataGridView.SelectionChanged += dataGridViewSelectionChanged;


            passwordTextBox.TextChanged += encryptTextbox;
            axKHOpenAPI1.CommConnect();
        }
        public void onReceiveRealCondition(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealConditionEvent e)
        {
            if (e.strType == "I")
            {
                string stockName = axKHOpenAPI1.GetMasterCodeName(e.sTrCode);
                insertListBox.Items.Add("종목편입| 조건인덱스 : " + e.strConditionIndex + " | 종목코드 : " + e.sTrCode + " | " + "종목명 : " + stockName);
                currentCondition = e.strConditionIndex + ":" + e.strConditionName;
                axKHOpenAPI1.SetInputValue("종목코드", e.sTrCode);
                axKHOpenAPI1.CommRqData("자동거래2차매수", "opt10001", 0, "5152");

            }
            else if (e.strType == "D")
            {
                string stockName = axKHOpenAPI1.GetMasterCodeName(e.sTrCode);
                deleteListBox.Items.Add("종목이탈| 조건인덱스 : " + e.strConditionIndex + " | 종목코드 : " + e.sTrCode + " | " + "종목명 : " + stockName);
            }


        }
        public void onReceiveTrCondition(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrConditionEvent e)
        {
            if (e.strCodeList.Length > 0)
            {
                string stockCodeList = e.strCodeList.Remove(e.strCodeList.Length - 1);
                int stockCount = stockCodeList.Split(';').Length;
                if (stockCount <= 100)
                {
                    axKHOpenAPI1.CommKwRqData(stockCodeList, 0, stockCount, 0, "조건검색종목", "5100");
                }
                if (e.nNext != 0)
                {
                    axKHOpenAPI1.SendCondition("5101", e.strConditionName, e.nIndex, 1);

                }
            }
            else if (e.strCodeList.Length == 0)
            {
                MessageBox.Show("검색된 종목이 없습니다.");
            }
        }
        public void dataGridViewSelectionChanged(object sender, EventArgs e)
        {
            if (sender.Equals(balanceDataGridView))
            {
                if (balanceDataGridView.SelectedCells.Count > 0)
                {
                    int rowIndex = balanceDataGridView.SelectedCells[0].RowIndex;
                    string[] currentPriceArray = balanceDataGridView["현재가", rowIndex].Value.ToString().Split(',');
                    string stockCode = balanceDataGridView["종목코드", rowIndex].Value.ToString().Replace("A", "").Trim();
                    string stockNumber = balanceDataGridView["수량", rowIndex].Value.ToString();
                    string currentPrice = "";
                    for (int i = 0; i < currentPriceArray.Length; i++)
                    {
                        currentPrice = currentPrice + currentPriceArray[i];
                    }
                    stockCodeLabel.Text = stockCode;
                    orderPriceNumericUpDown.Value = long.Parse(currentPrice);
                    orderNumberNumericUpDown.Value = long.Parse(stockNumber);
                }
            }
            else if (sender.Equals(outstandingDataGridView))
            {
                if (outstandingDataGridView.SelectedCells.Count > 0)
                {
                    int rowIndex = outstandingDataGridView.SelectedCells[0].RowIndex;
                    string[] outstandingPriceArray = outstandingDataGridView["주문가격", rowIndex].Value.ToString().Split(',');
                    string outstandingStockCode = outstandingDataGridView["종목코드", rowIndex].Value.ToString();
                    string outstandingStockNumber = outstandingDataGridView["미체결수량", rowIndex].Value.ToString();
                    string outstandingPrice = "";
                    for (int i = 0; i < outstandingPriceArray.Length; i++)
                    {
                        outstandingPrice = outstandingPrice + outstandingPriceArray[i];
                    }
                    stockCodeLabel.Text = outstandingStockCode;
                    orderPriceNumericUpDown.Value = long.Parse(outstandingPrice);
                    orderNumberNumericUpDown.Value = long.Parse(outstandingStockNumber);
                }
            }
        }
        public void onReceiveChejanData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            if (e.sGubun == "0")//주문 접수 , 체결시
            {
                string orderNumber = axKHOpenAPI1.GetChejanData(9203);
                string orderStatus = axKHOpenAPI1.GetChejanData(913);
                string orderStockName = axKHOpenAPI1.GetChejanData(302);
                string orderStockNumber = axKHOpenAPI1.GetChejanData(900);
                long orderPrice = long.Parse(axKHOpenAPI1.GetChejanData(901));
                string orderType = axKHOpenAPI1.GetChejanData(905);

                orderRecordListBox.Items.Add("주문번호 : " + orderNumber + " | " + "주문상태 : " + orderStatus);
                orderRecordListBox.Items.Add("종목명 : " + orderStockName + " | " + "주문수량 : " + orderStockNumber);
                orderRecordListBox.Items.Add("주문가격 : " + String.Format("{0:#,###}", orderPrice));
                orderRecordListBox.Items.Add("주문구분 : " + orderType);
                orderRecordListBox.Items.Add("----------------------------------------------------");

            }
            else if (e.sGubun == "1")//국내주식 잔고전달
            {
                string stockName = axKHOpenAPI1.GetChejanData(302);
                long currentPrice = long.Parse(axKHOpenAPI1.GetChejanData(10).Replace("-", ""));

                string profitRate = axKHOpenAPI1.GetChejanData(8019);
                long totalBuyingPrice = long.Parse(axKHOpenAPI1.GetChejanData(932));
                long profitMoney = long.Parse(axKHOpenAPI1.GetChejanData(950));

                todayProfitLabel.Text = String.Format("{0:#,###}", profitMoney);
                todayProfitRateLabel.Text = profitRate;

            }
        }
        public void encryptTextbox(object sender, EventArgs e)
        {
            if (sender.Equals(passwordTextBox))
            {
                passwordTextBox.PasswordChar = '*';
                passwordTextBox.MaxLength = 4;
            }
        }

        public void onReceiveRealData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEvent e)
        {

            for (int i = 0; i < autoTradingRuleList.Count; i++)
            {
                if (autoTradingRuleList[i].상태 == "시작")
                {
                    double profitRate = autoTradingRuleList[i].이익률 * 0.01;
                    double lossRate = autoTradingRuleList[i].손절률 * 0.01;

                    for (int j = 0; j < autoTradingRuleList[i].autoTradingPurchaseStockList.Count; j++)
                    {
                        autoTradingRuleList[i].autoTradingPurchaseStockList[j].currentPrice = int.Parse(axKHOpenAPI1.GetCommRealData(e.sRealKey, 10));
                        autoTradingRuleList[i].autoTradingPurchaseStockList[j].boughtCount = int.Parse(axKHOpenAPI1.GetCommRealData(e.sRealKey, 930));

                        string stockCode = autoTradingRuleList[i].autoTradingPurchaseStockList[j].stockCode;
                        int currentPrice = autoTradingRuleList[i].autoTradingPurchaseStockList[j].currentPrice;
                        int boughtPrice = autoTradingRuleList[i].autoTradingPurchaseStockList[j].boughtPrice;
                        int boughtCount = autoTradingRuleList[i].autoTradingPurchaseStockList[j].boughtCount;
                        string orderType = autoTradingRuleList[i].매도_거래구분;
                        string[] orderTypeArray = orderType.Split(':');

                        if (currentPrice == (boughtPrice + (boughtPrice * profitRate)))
                        {
                            axKHOpenAPI1.SendOrder("이익율매도주문", "8889", ACCOUNT_NUMBER, 2, stockCode, boughtCount, currentPrice, orderTypeArray[0], "");
                        }
                        else if (currentPrice == (boughtPrice - (boughtPrice * profitRate)))
                        {
                            axKHOpenAPI1.SendOrder("손절율매도주문", "8789", ACCOUNT_NUMBER, 2, stockCode, boughtCount, currentPrice, orderTypeArray[0], "");
                        }

                    }
                }
            }
        }
        public void onReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (e.sRQName == "계좌평가잔고내역요청")
            {
                long totalBuyingAmount = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "총매입금액"));
                long totalEstimatedAmount = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "총평가금액"));

                totalBuyLabel.Text = String.Format("{0:#,###}", totalBuyingAmount);
                totalEstimateLabel.Text = String.Format("{0:#,###}", totalEstimatedAmount);
            }
            else if (e.sRQName == "계좌평가현황요청")
            {
                long deposit = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "예수금"));
                long todayProfit = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "당일투자손익"));
                double todayProfitRate = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "당일손익율"));

                depositLabel.Text = String.Format("{0:#,###}", deposit);
                todayProfitLabel.Text = String.Format("{0:#,###}", todayProfit);
                todayProfitRateLabel.Text = String.Format("{0:#.##}", todayProfitRate);

                int count = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);
                stockBalanceList = new List<stockBalance>();
                for (int i = 0; i < count; i++)
                {
                    string stockCode = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").TrimStart('0');
                    string stockName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                    long number = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "보유수량"));
                    long buyingMoney = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "매입금액"));
                    long currentPrice = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Replace("-", ""));
                    long estimatedProfit = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "손익금액"));
                    double estimatedProfitRate = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "손익율"));

                    stockBalanceList.Add(new stockBalance(stockCode, stockName, number, String.Format("{0:#,###}", buyingMoney), String.Format("{0:#,###}", currentPrice), estimatedProfit, String.Format("{0:f2}", estimatedProfitRate)));
                }
                balanceDataGridView.DataSource = stockBalanceList;
            }
            else if (e.sRQName == "실시간미체결요청")
            {
                int count = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);
                outstandingList = new List<outstanding>();
                for (int i = 0; i < count; i++)
                {
                    string orderCode = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문번호")).ToString();
                    string stockCode = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                    string stockName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                    int orderNumber = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문수량"));
                    int orderPrice = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문가격"));
                    int outstandingNumber = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "미체결수량"));
                    int currentPrice = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Replace("-", ""));
                    string orderGubun = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문구분").Trim();
                    string orderTime = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "시간").Trim();

                    outstandingList.Add(new outstanding(orderCode, stockCode, stockName, orderNumber, String.Format("{0:#,###}", orderPrice), String.Format("{0:#,###}", currentPrice), outstandingNumber, orderGubun, orderTime));

                }
                outstandingDataGridView.DataSource = outstandingList;
            }
            else if (e.sRQName == "종목정보요청")
            {
                string currentStockPrice = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "현재가");
                orderPriceNumericUpDown.Value = long.Parse(currentStockPrice.Replace("-", ""));
            }
            else if (e.sRQName == "조건검색종목")
            {
                int count = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);//조건식으로 검색되는 종목의 개수

                if (autoRuleDataGridView.SelectedCells.Count > 0)//선택된 DataGridView 셀을 체크
                {
                    int rowIndex = autoRuleDataGridView.SelectedCells[0].RowIndex;

                    int autoTradingRuleID = int.Parse(autoRuleDataGridView["거래규칙_번호", rowIndex].Value.ToString());
                    int autoOrderPricePerStock = int.Parse(autoRuleDataGridView["거래규칙_종목당_매수금액", rowIndex].Value.ToString());
                    int autoTradingLimitOrderNumber = int.Parse(autoRuleDataGridView["거래규칙_매입제한_종목_개수", rowIndex].Value.ToString());

                    string autoBuyOrderType = autoRuleDataGridView["거래규칙_매수_거래구분", rowIndex].Value.ToString();
                    string[] autoBuyOrderArray = autoBuyOrderType.Split(':');

                    string autoRuleStatus = autoRuleDataGridView["거래규칙_상태", rowIndex].Value.ToString();

                    int autoRuleListIndex = autoTradingRuleList.FindIndex(o => o.번호 == autoRuleID);


                    if (autoRuleStatus == "시작" && accountComboBox.Text.Length > 0)//거래규칙 상태가 "시작"이면
                    {
                        ACCOUNT_NUMBER = accountComboBox.Text;
                        for (int i = 0; i < count; i++)
                        {
                            if (i > autoTradingLimitOrderNumber)//제한 종목개수 초과하면 break;
                            {

                                break;
                            }
                            else
                            {
                                string stockCode = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                                string stockName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                                int stockPrice = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가").Replace("-", ""));

                                if (autoOrderPricePerStock > stockPrice)
                                {
                                    int orderNumber = autoOrderPricePerStock / stockPrice;
                                    axKHOpenAPI1.SendOrder("자동거래매수주문", "5149", ACCOUNT_NUMBER, 1, stockCode, orderNumber, stockPrice, autoBuyOrderArray[0], "");
                                    autoTradingRuleList[autoRuleListIndex].autoTradingPurchaseStockList.Add(new AutoTradingPurchaseStock(stockCode, stockPrice, 0));

                                }
                            }
                        }
                    }
                }
            }
            else if (e.sRQName == "자동거래2차매수")
            {
                int stockPrice = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "현재가").Replace("-", ""));
                string stockCode = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목코드").Trim();
                int ruleIndex = autoTradingRuleList.FindIndex(o => o.조건식이름 == currentCondition);

                if (autoTradingRuleList[ruleIndex].autoTradingPurchaseStockList.Count < autoTradingRuleList[ruleIndex].매입제한_종목_개수 && autoTradingRuleList[ruleIndex].종목당_매수금액 > stockPrice)
                {

                    if (accountComboBox.Text.Length > 0 && autoTradingRuleList[ruleIndex].상태 == "시작")
                    {
                        int autoOrderPricePerStock = autoTradingRuleList[ruleIndex].종목당_매수금액;
                        int orderNumber = autoOrderPricePerStock / stockPrice;
                        ACCOUNT_NUMBER = accountComboBox.Text;
                        string[] autoOrderTypeArray = autoTradingRuleList[ruleIndex].매수_거래구분.Split(':');

                        axKHOpenAPI1.SendOrder("자동거래2차매수주문", "5154", ACCOUNT_NUMBER, 1, stockCode, orderNumber, stockPrice, autoOrderTypeArray[0], "");
                        autoTradingRuleList[ruleIndex].autoTradingPurchaseStockList.Add(new AutoTradingPurchaseStock(stockCode, stockPrice, 0));
                    }
                }
            }
        }
        public void setBuyingPerStock(object sender, EventArgs e)
        {
            if (sender.Equals(limitPriceNumericUpDown))
            {
                long limitPrice = long.Parse(limitPriceNumericUpDown.Value.ToString());
                long limitNumber = long.Parse(limitNumberNumericUpDown.Value.ToString());
                if (limitPrice > 0 && limitNumber > 0)
                {
                    long limitBuyingPerStock = limitPrice / limitNumber;
                    limitBuyingPerStockLabel.Text = limitBuyingPerStock.ToString();
                }
            }
            else if (sender.Equals(limitNumberNumericUpDown))
            {
                long limitPrice = long.Parse(limitPriceNumericUpDown.Value.ToString());
                long limitNumber = long.Parse(limitNumberNumericUpDown.Value.ToString());
                if (limitPrice > 0 && limitNumber > 0)
                {
                    long limitBuyingPerStock = limitPrice / limitNumber;
                    limitBuyingPerStockLabel.Text = limitBuyingPerStock.ToString();
                }
            }
        }
        public void ButtonClicked(object sender, EventArgs e)
        {
            if (sender.Equals(setAutoTradingRuleButton))
            {
                string selectedCondition = conditionComboBox.Text;//조건식 선택
                int limitBuyingStockPrice = int.Parse(limitPriceNumericUpDown.Value.ToString());//매입제한 금액
                int limitBuyingStockNumber = int.Parse(limitNumberNumericUpDown.Value.ToString());//매입 제한 종목개수
                int limitBuyingPerStock = int.Parse(limitBuyingPerStockLabel.Text.ToString());//종목당 매수금액
                string autoBuyingOrderType = autoBuyOrderComboBox.Text;//매수 거래구분

                double profitRate = double.Parse(limitProfitRateNumericUpDown.Value.ToString());//이익률
                double lossRate = double.Parse(limitLossNumericUpDown.Value.ToString());//손절률
                string autoSellingOrderType = autoSellOrderComboBox.Text;//매도 거래구분
                string status = "정지";

                if (selectedCondition.Length > 0 && limitBuyingStockPrice > 0 && limitBuyingStockNumber > 0 &&
                    limitBuyingPerStock > 0 && autoBuyingOrderType.Length > 0 && profitRate > 0 &&
                    lossRate != 0 && autoSellingOrderType.Length > 0)
                {

                    autoRuleID++;
                    string[] conditionName = selectedCondition.Split(':');
                    autoTradingRuleList.Add(new AutoTradingRule(autoRuleID, conditionName[1], limitBuyingStockPrice, limitBuyingStockNumber,
                        limitBuyingPerStock, autoBuyingOrderType, autoSellingOrderType, profitRate, lossRate, status));
                    //autoRuleDataGridView.Rows.Add();
                    //autoRuleDataGridView.Rows.Add(autoTradingRuleList);
                    //autoRuleDataGridView["거래규칙_번호", autoRuleDataGridView.Rows.Count - 1].Value = autoRuleID;
                    //autoRuleDataGridView["거래규칙_조건식", autoRuleDataGridView.Rows.Count - 1].Value = selectedCondition;
                    //autoRuleDataGridView["거래규칙_매입제한_금액", autoRuleDataGridView.Rows.Count - 1].Value = limitBuyingStockPrice;
                    //autoRuleDataGridView["거래규칙_매입제한_종목_개수", autoRuleDataGridView.Rows.Count - 1].Value = limitBuyingStockNumber;
                    //autoRuleDataGridView["거래규칙_종목당_매수금액", autoRuleDataGridView.Rows.Count - 1].Value = limitBuyingPerStock;
                    //autoRuleDataGridView["거래규칙_매수_거래구분", autoRuleDataGridView.Rows.Count - 1].Value = autoBuyingOrderType;
                    //autoRuleDataGridView["거래규칙_매도_거래구분", autoRuleDataGridView.Rows.Count - 1].Value = autoSellingOrderType;
                    //autoRuleDataGridView["거래규칙_이익률", autoRuleDataGridView.Rows.Count - 1].Value = profitRate;
                    //autoRuleDataGridView["거래규칙_손절률", autoRuleDataGridView.Rows.Count - 1].Value = lossRate;
                    //autoRuleDataGridView["거래규칙_상태", autoRuleDataGridView.Rows.Count - 1].Value = status;

                    autoRuleDataGridView.ColumnCount = 10;
                    autoRuleDataGridView.Columns[0].Name = "거래규칙_번호";
                    autoRuleDataGridView.Columns[1].Name = "거래규칙_조건식";
                    autoRuleDataGridView.Columns[2].Name = "거래규칙_매입제한_금액";
                    autoRuleDataGridView.Columns[3].Name = "거래규칙_매입제한_종목_개수";
                    autoRuleDataGridView.Columns[4].Name = "거래규칙_종목당_매수금액";
                    autoRuleDataGridView.Columns[5].Name = "거래규칙_매수_거래구분";
                    autoRuleDataGridView.Columns[6].Name = "거래규칙_매도_거래구분";
                    autoRuleDataGridView.Columns[7].Name = "거래규칙_이익률";
                    autoRuleDataGridView.Columns[8].Name = "거래규칙_손절률";
                    autoRuleDataGridView.Columns[9].Name = "거래규칙_상태";

                    autoRuleDataGridView.Rows.Add(autoTradingRuleList);
                    //autoRuleDataGridView.Rows.Add();
                    for (int i = 0; i < autoRuleID; i++)
                    {
                        autoRuleDataGridView["거래규칙_번호", i ].Value = autoTradingRuleList[i].번호;
                        autoRuleDataGridView["거래규칙_조건식", i ].Value = autoTradingRuleList[i].조건식이름;
                        autoRuleDataGridView["거래규칙_매입제한_금액", i].Value = autoTradingRuleList[i].매입제한_금액;
                        autoRuleDataGridView["거래규칙_매입제한_종목_개수", i].Value = autoTradingRuleList[i].매입제한_종목_개수;
                        autoRuleDataGridView["거래규칙_종목당_매수금액", i].Value = autoTradingRuleList[i].종목당_매수금액;
                        autoRuleDataGridView["거래규칙_매수_거래구분", i ].Value = autoTradingRuleList[i].매수_거래구분;
                        autoRuleDataGridView["거래규칙_매도_거래구분", i ].Value = autoTradingRuleList[i].매도_거래구분;
                        autoRuleDataGridView["거래규칙_이익률", i].Value = autoTradingRuleList[i].이익률;
                        autoRuleDataGridView["거래규칙_손절률", i].Value = autoTradingRuleList[i].손절률;
                        autoRuleDataGridView["거래규칙_상태", i].Value = autoTradingRuleList[i].상태;
                    }

                    MessageBox.Show("설정완료");
                }
                else if (selectedCondition.Length == 0 || limitBuyingStockPrice == 0 || limitBuyingStockNumber == 0 ||
                    limitBuyingPerStock == 0 || autoBuyingOrderType.Length == 0 || profitRate == 0 ||
                    lossRate == 0 || autoSellingOrderType.Length == 0)
                {
                    MessageBox.Show("거래규칙 값을 모두 입력하세요");
                }
            }
            else if (sender.Equals(stockSearchButton))
            {
                string stockName = stockTextBox.Text;
                int index = stockList.FindIndex(o => o.stockName == stockName);
                string stockCode = stockList[index].stockCode;
                stockCodeLabel.Text = stockCode;

                axKHOpenAPI1.SetInputValue("종목코드", stockCode);
                axKHOpenAPI1.CommRqData("종목정보요청", "opt10001", 0, "5000");

            }
            else if (sender.Equals(balanceCheckButton))
            {
                if (accountComboBox.Text.Length > 0 && passwordTextBox.Text.Length > 0)
                {
                    ACCOUNT_NUMBER = accountComboBox.Text;
                    string password = passwordTextBox.Text;

                    axKHOpenAPI1.SetInputValue("계좌번호", ACCOUNT_NUMBER);
                    axKHOpenAPI1.SetInputValue("비밀번호", password);
                    axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00");
                    axKHOpenAPI1.SetInputValue("조회구분", "1");
                    axKHOpenAPI1.CommRqData("계좌평가잔고내역요청", "opw00018", 0, "8100");

                    axKHOpenAPI1.SetInputValue("계좌번호", ACCOUNT_NUMBER);
                    axKHOpenAPI1.SetInputValue("비밀번호", password);
                    axKHOpenAPI1.SetInputValue("상장폐지조회구분", "0");
                    axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00");
                    axKHOpenAPI1.CommRqData("계좌평가현황요청", "opw00004", 0, "4000");

                    axKHOpenAPI1.SetInputValue("계좌번호", ACCOUNT_NUMBER);
                    axKHOpenAPI1.SetInputValue("체결구분", "1");
                    axKHOpenAPI1.SetInputValue("매매구분", "2");
                    axKHOpenAPI1.CommRqData("실시간미체결요청", "opt10075", 0, "5700");

                }
            }
            else if (sender.Equals(buyButton))
            {
                string buyOrderType = orderComboBox.Text;
                int buyOrderPrice = int.Parse(orderPriceNumericUpDown.Value.ToString());
                int buyOrderNumber = int.Parse(orderNumberNumericUpDown.Value.ToString());
                ACCOUNT_NUMBER = accountComboBox.Text;
                string stockCode = stockCodeLabel.Text;

                if (buyOrderType.Length > 0 && buyOrderPrice > 0 && buyOrderNumber > 0 && ACCOUNT_NUMBER.Length > 0 && stockCode.Length > 0)
                {
                    string[] orderType = buyOrderType.Split(':');
                    axKHOpenAPI1.SendOrder("신규종목매수주문", "8249", ACCOUNT_NUMBER, 1, stockCode, buyOrderNumber, buyOrderPrice, orderType[0], "");
                }
            }
            else if (sender.Equals(sellButton))
            {
                ACCOUNT_NUMBER = accountComboBox.Text;//계좌번호
                string stockCode = stockCodeLabel.Text;//종목코드
                string sellOrderType = orderComboBox.Text;//거래구분
                int sellOrderPrice = int.Parse(orderPriceNumericUpDown.Value.ToString());
                int sellOrderNumber = int.Parse(orderNumberNumericUpDown.Value.ToString());

                if (ACCOUNT_NUMBER.Length > 0 && sellOrderType.Length > 0 && stockCode.Length > 0 && sellOrderPrice > 0 && sellOrderNumber > 0)
                {
                    string[] orderType = sellOrderType.Split(':');
                    axKHOpenAPI1.SendOrder("신규종목매도주문", "8289", ACCOUNT_NUMBER, 2, stockCode, sellOrderNumber, sellOrderPrice, orderType[0], "");
                }
            }
            else if (sender.Equals(orderFixButton))
            {
                ACCOUNT_NUMBER = accountComboBox.Text;
                if (outstandingDataGridView.SelectedCells.Count > 0 && ACCOUNT_NUMBER.Length > 0)
                {
                    int rowIndex = outstandingDataGridView.SelectedCells[0].RowIndex;
                    string orderType = outstandingDataGridView["주문구분", rowIndex].Value.ToString();
                    string tradingType = orderComboBox.Text;
                    string stockCode = outstandingDataGridView["종목코드", rowIndex].Value.ToString();
                    int orderNumber = int.Parse(orderNumberNumericUpDown.Value.ToString());
                    int orderPrice = int.Parse(orderPriceNumericUpDown.Value.ToString());
                    string orderCode = outstandingDataGridView["주문번호", rowIndex].Value.ToString();

                    if (orderType.Length > 0 && tradingType.Length > 0 && stockCode.Length > 0 && orderNumber > 0 && orderPrice > 0 && orderCode.Length > 0)
                    {
                        string[] tradingTypeArray = tradingType.Split(':');
                        if (orderType == "-매도")
                        {
                            axKHOpenAPI1.SendOrder("종목주문정정", "1430", ACCOUNT_NUMBER, 6, stockCode, orderNumber, orderPrice, tradingTypeArray[0], orderCode);
                            MessageBox.Show("정정요청 완료");
                        }
                        else if (orderType == "+매수")
                        {
                            axKHOpenAPI1.SendOrder("종목주문정정", "1430", ACCOUNT_NUMBER, 5, stockCode, orderNumber, orderPrice, tradingTypeArray[0], orderCode);
                            MessageBox.Show("정정요청 완료");
                        }
                    }
                }
            }
            else if (sender.Equals(orderCancelButton))
            {
                if (outstandingDataGridView.SelectedCells.Count > 0)
                {
                    ACCOUNT_NUMBER = accountComboBox.Text;
                    if (outstandingDataGridView.SelectedCells.Count > 0 && ACCOUNT_NUMBER.Length > 0)
                    {
                        int rowIndex = outstandingDataGridView.SelectedCells[0].RowIndex;
                        string orderType = outstandingDataGridView["주문구분", rowIndex].Value.ToString();
                        string tradingType = orderComboBox.Text;
                        string stockCode = outstandingDataGridView["종목코드", rowIndex].Value.ToString();
                        int orderNumber = int.Parse(orderNumberNumericUpDown.Value.ToString());
                        int orderPrice = int.Parse(orderPriceNumericUpDown.Value.ToString());
                        string orderCode = outstandingDataGridView["주문번호", rowIndex].Value.ToString();

                        if (orderType.Length > 0 && tradingType.Length > 0 && stockCode.Length > 0 && orderNumber > 0 && orderPrice > 0 && orderCode.Length > 0)
                        {
                            string[] tradingTypeArray = tradingType.Split(':');
                            if (orderType == "-매도")
                            {
                                axKHOpenAPI1.SendOrder("종목주문정정", "1430", ACCOUNT_NUMBER, 4, stockCode, orderNumber, orderPrice, tradingTypeArray[0], orderCode);
                                MessageBox.Show("취소요청 완료");
                            }
                            else if (orderType == "+매수")
                            {
                                axKHOpenAPI1.SendOrder("종목주문정정", "1430", ACCOUNT_NUMBER, 3, stockCode, orderNumber, orderPrice, tradingTypeArray[0], orderCode);
                                MessageBox.Show("취소요청 완료");
                            }
                        }
                    }
                }
            }
            else if (sender.Equals(autoTradingStartButton))
            {
                if (autoRuleDataGridView.SelectedCells.Count > 0)
                {
                    //예외처리해야함 v0.1
         

                    int rowIndex = autoRuleDataGridView.SelectedCells[0].RowIndex;
                    autoRuleDataGridView["거래규칙_상태", rowIndex].Value = "시작";

                    string autoTradingCondition = autoRuleDataGridView["거래규칙_조건식", rowIndex].Value.ToString();
                    string[] autoTradingArray = autoTradingCondition.Split(':');
                    autoSreenNumber++;
                    string scrNumber = autoSreenNumber.ToString();

                    axKHOpenAPI1.SendCondition(scrNumber, autoTradingArray[1], int.Parse(autoTradingArray[0]), 0);
                    axKHOpenAPI1.SendCondition(scrNumber, autoTradingArray[1], int.Parse(autoTradingArray[0]), 1);
                }
            }
            else if (sender.Equals(autoTradingStopButton))
            {
                if (autoRuleDataGridView.SelectedCells.Count > 0)
                {
                    int rowIndex = autoRuleDataGridView.SelectedCells[0].RowIndex;
                    autoRuleDataGridView["거래규칙_상태", rowIndex].Value = "정지";

                }
            }
            else if (sender.Equals(SellAllStockButton))
            {
                for (int i = 0; i < autoTradingRuleList.Count; i++)
                {
                    for (int j = 0; j < autoTradingRuleList[i].autoTradingPurchaseStockList.Count; j++)
                    {
                        string stockCode = autoTradingRuleList[i].autoTradingPurchaseStockList[j].stockCode;
                        int boughtCount = autoTradingRuleList[i].autoTradingPurchaseStockList[j].boughtCount;
                        int currentPrice = autoTradingRuleList[i].autoTradingPurchaseStockList[j].boughtPrice;
                        axKHOpenAPI1.SendOrder("전체청산주문", "9999", ACCOUNT_NUMBER, 2, stockCode, boughtCount, currentPrice, "03", "");
                    }
                }
            }
        }
        public void onReceiveConditionVer(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveConditionVerEvent e)
        {
            if (e.lRet == 1)
            {
                string conditionList = axKHOpenAPI1.GetConditionNameList().TrimEnd(';');
                string[] conditionArray = conditionList.Split(';');

                for (int i = 0; i < conditionArray.Length; i++)
                {
                    string[] condition = conditionArray[i].Split('^');
                    conditionComboBox.Items.Add(condition[0] + ":" + condition[1]);

                }
            }
        }
        public void onEventConnect(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            if (e.nErrCode == 0)
            {
                stockList = new List<StockInfo>();
                autoTradingRuleList = new List<AutoTradingRule>();

                orderComboBox.Items.Add("00:지정가".ToString());
                orderComboBox.Items.Add("03:시장가".ToString());

                autoBuyOrderComboBox.Items.Add("00:지정가".ToString());
                autoBuyOrderComboBox.Items.Add("03:시장가".ToString());

                autoSellOrderComboBox.Items.Add("00:지정가".ToString());
                autoSellOrderComboBox.Items.Add("03:시장가".ToString());

                string accountList = axKHOpenAPI1.GetLoginInfo("ACCLIST");
                string[] accountArray = accountList.Split(';');
                for (int i = 0; i < accountArray.Length; i++)
                {
                    accountComboBox.Items.Add(accountArray[i]);
                }
                string stocklist = axKHOpenAPI1.GetCodeListByMarket(null);
                string[] stockArray = stocklist.Split(';');
                AutoCompleteStringCollection stockCollection = new AutoCompleteStringCollection();

                for (int i = 0; i < stockArray.Length; i++)
                {
                    stockList.Add(new StockInfo(stockArray[i], axKHOpenAPI1.GetMasterCodeName(stockArray[i])));
                    stockCollection.Add(axKHOpenAPI1.GetMasterCodeName(stockArray[i]));
                }
                stockTextBox.AutoCompleteCustomSource = stockCollection;
                axKHOpenAPI1.GetConditionLoad();

            }
        }
    }
    class StockItemObject
    {

    }
    class StockInfo
    {
        public string stockCode;
        public string stockName;
        public StockInfo(string stockCode, string stockName)
        {
            this.stockCode = stockCode;
            this.stockName = stockName;
        }
    }
    class ConditionObject
    {
        public string conditionIndex;
        public string conditionName;

        public ConditionObject(string conditionIndex, string conditionName)
        {
            this.conditionIndex = conditionIndex;
            this.conditionName = conditionName;

        }
    }
    class AutoTradingRule
    {
        public int 번호;
        public string 조건식이름;
        public long 매입제한_금액;
        public int 매입제한_종목_개수;
        public int 종목당_매수금액;
        public string 매수_거래구분;
        public string 매도_거래구분;
        public double 이익률;
        public double 손절률;
        public string 상태;

        public List<AutoTradingPurchaseStock> autoTradingPurchaseStockList;

        public AutoTradingRule(int autoTradingRuleID, string conditionName, long limitBuyingStockPrice, int limitBuyingStockNumber, int limitBuyingPerStock, string autoBuyingOrderType, string autoSellingOrderType, double profitRate, double lossRate, string status)
        {
            this.번호 = autoTradingRuleID;
            this.조건식이름 = conditionName;
            this.매입제한_금액 = limitBuyingStockPrice;
            this.매입제한_종목_개수 = limitBuyingStockNumber;
            this.종목당_매수금액 = limitBuyingPerStock;
            this.매수_거래구분 = autoBuyingOrderType;
            this.매도_거래구분 = autoSellingOrderType;
            this.이익률 = profitRate;
            this.손절률 = lossRate;
            this.상태 = status;
            this.autoTradingPurchaseStockList = new List<AutoTradingPurchaseStock>();
        }
    }
    class AutoTradingPurchaseStock
    {
        public string stockCode;
        public int boughtPrice;
        public int boughtCount;
        public int currentPrice;

        public AutoTradingPurchaseStock(string stockCode, int boughtPrice, int currentPrice)
        {
            this.stockCode = stockCode;
            this.boughtPrice = boughtPrice;
            this.currentPrice = currentPrice;
        }


    }

    class outstanding
    {
        public string 주문번호 { get; set; }
        public string 종목코드 { get; set; }
        public string 종목명 { get; set; }
        public int 주문수량 { get; set; }
        public string 주문가격 { get; set; }
        public int 미체결수량 { get; set; }
        public string 주문구분 { get; set; }
        public string 현재가 { get; set; }
        public string 시간 { get; set; }

        public outstanding()
        {

        }
        public outstanding(string 주문번호, string 종목코드, string 종목명, int 주문수량, string 주문가격, string 현재가, int 미체결수량, string 주문구분, string 시간)
        {
            this.주문번호 = 주문번호;
            this.종목코드 = 종목코드;
            this.종목명 = 종목명;
            this.주문수량 = 주문수량;
            this.주문가격 = 주문가격;
            this.미체결수량 = 미체결수량;
            this.주문구분 = 주문구분;
            this.현재가 = 현재가;
            this.시간 = 시간;

        }
    }
    class stockBalance
    {
        public string 종목코드 { get; set; }
        public string 종목명 { get; set; }
        public long 수량 { get; set; }
        public string 매수금 { get; set; }
        public string 현재가 { get; set; }
        public long 평가손익 { get; set; }
        public string 수익률 { get; set; }

        public stockBalance() { }

        public stockBalance(string 종목번호, string 종목명, long 수량, string 매수금, string 현재가, long 평가손익, string 수익률)
        {
            this.종목코드 = 종목번호;
            this.종목명 = 종목명;
            this.수량 = 수량;
            this.매수금 = 매수금;
            this.현재가 = 현재가;
            this.평가손익 = 평가손익;
            this.수익률 = 수익률;
        }
    }
}
