using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PickupMenifest.Model;
using PickupMenifest.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace CourierTrackingService
{
    class Program
    {
        public static int delaytime = 0;

        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
            delaytime = Convert.ToInt32(config.GetSection("MySettings").GetSection("IntervalInMinutes").Value);
            Thread _Individualprocessthread = new Thread(new ThreadStart(InvokeMethod));
            _Individualprocessthread.Start();
        }
        public static void InvokeMethod()
        {
            while (true)
            {
                GetConnectionStrings();
              
                Thread.Sleep(delaytime);
            }
        }
        /// <summary>
        /// GetConnectionStrings
        /// </summary>
        public static void GetConnectionStrings()
        {
            string ServerName = string.Empty;
            string ServerCredentailsUsername = string.Empty;
            string ServerCredentailsPassword = string.Empty;
            string DBConnection = string.Empty;
            try
            {
                DataTable dt = new DataTable();
                IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
                var constr = config.GetSection("ConnectionStrings").GetSection("HomeShop").Value;
                MySqlConnection con = new MySqlConnection(constr);
                MySqlCommand cmd = new MySqlCommand("SP_HSGetAllConnectionstrings", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Connection.Open();
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(dt);
                cmd.Connection.Close();

                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        ServerName = Convert.ToString(dr["ServerName"]);
                        ServerCredentailsUsername = Convert.ToString(dr["ServerCredentailsUsername"]);
                        ServerCredentailsPassword = Convert.ToString(dr["ServerCredentailsPassword"]);
                        DBConnection = Convert.ToString(dr["DBConnection"]);

                        string ConString = "Data Source = " + ServerName + " ; port = " + 3306 + "; Initial Catalog = " + DBConnection + " ; User Id = " + ServerCredentailsUsername + "; password = " + ServerCredentailsPassword + "";
                        GetdataFromMySQL(ConString);
                    }
                }
            }
            catch(Exception ex)
            {


            }
            finally
            {
               
                GC.Collect();
            }


        }

        /// <summary>
        /// GetdataFromMySQL
        /// </summary>
        /// <param name="ConString"></param>
        public static void GetdataFromMySQL(string ConString)
        {
            int ID = 0;
            int shipment_id = 0;
            int TenantId = 0;
            string AWBNo = string.Empty;
            string InvoiceNo = string.Empty;
            string apiResponse = string.Empty;
            string apiGenPickupRes = string.Empty;
            string apiGenMenifestRes = string.Empty;
            string StoreCode = string.Empty;

            PickupResponce pickupResponce = new PickupResponce();
            pickupResponce.response = new response();
            ManifestResponce manifestResponce = new ManifestResponce();

            MySqlConnection con = null;
            try
            {
                DataTable dt = new DataTable();

                IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
               
                string ClientAPIURL = config.GetSection("MySettings").GetSection("ClientAPIURL").Value;


                con = new MySqlConnection(ConString);
                MySqlCommand cmd = new MySqlCommand("SP_PHYGetPickupMenifestDetails", con)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                cmd.Connection.Open();
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(dt);
                cmd.Connection.Close();
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        ID = Convert.ToInt32(dr["ID"]);
                        InvoiceNo = Convert.ToString(dr["InvoiceNo"]);
                        shipment_id = Convert.ToInt32(dr["CourierPartnerShipmentID"]);
                        TenantId = Convert.ToInt32(dr["TenantId"]);
                        StoreCode = Convert.ToString(dr["StoreCode"]);

                        PickupManifestRequest pickupManifestRequest = new PickupManifestRequest()
                        {
                            shipmentId = new List<int> { Convert.ToInt32(shipment_id) }
                        };

                        try
                        {
                            string apiGenPickupReq = JsonConvert.SerializeObject(pickupManifestRequest);
                            apiGenPickupRes = CommonService.SendApiRequest(ClientAPIURL + "/api/ShoppingBag/GeneratePickup", apiGenPickupReq);
                            pickupResponce = JsonConvert.DeserializeObject<PickupResponce>(apiGenPickupRes);
                            if (pickupResponce.status_code == 0 && pickupResponce.pickupStatus == "1")
                            {
                                if (pickupResponce.response != null)
                                {
                                    if (pickupResponce.response.pickupTokenNumber != null)
                                    {
                                        
                                        UpdateGeneratePickupManifest(ID, TenantId, ID, "Pickup", ConString);
                                    }
                                }
                            }
                            else
                            {
                                ExLogger(ID, InvoiceNo, Convert.ToString(DateTime.Now), StoreCode, pickupResponce.status_code+" : "+pickupResponce.message, apiGenPickupRes, ConString);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        try
                        {
                            string apiGenMenifestReq = JsonConvert.SerializeObject(pickupManifestRequest);
                            apiGenMenifestRes = CommonService.SendApiRequest(ClientAPIURL + "/api/ShoppingBag/GenerateManifest", apiGenMenifestReq);
                            manifestResponce = JsonConvert.DeserializeObject<ManifestResponce>(apiGenMenifestRes);
                            if (manifestResponce.status_code == 0)
                            {
                                if (manifestResponce.status == "1" && manifestResponce.manifestUrl != null && manifestResponce.manifestUrl != "")
                                {
                                    UpdateGeneratePickupManifest(ID, TenantId, ID, "Manifest", ConString);
                                }
                                else
                                {
                                    ExLogger(ID, InvoiceNo, Convert.ToString(DateTime.Now), StoreCode, manifestResponce.status, apiGenMenifestRes, ConString);
                                }
                            }else
                            {
                                ExLogger(ID, InvoiceNo, Convert.ToString(DateTime.Now), StoreCode, manifestResponce.status_code + " : " + manifestResponce.message, apiGenMenifestRes, ConString);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
                GC.Collect();
            }
        }



        public static void UpdateGeneratePickupManifest(int orderID, int tenantID, int userID, string status, string ConString)
        {

            try
            {
                DataTable dt = new DataTable();
                
                MySqlConnection con = new MySqlConnection(ConString);
                MySqlCommand cmd = new MySqlCommand("SP_PHYUpdateflagPickupManifest", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@order_ID", orderID);
                cmd.Parameters.AddWithValue("@_status", status);
                cmd.Parameters.AddWithValue("@tenant_ID", tenantID);
                cmd.Parameters.AddWithValue("@user_ID", userID);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch(Exception ex)
            {

            }
            finally
            {
                GC.Collect();
            }

        }
        /// <summary>
        /// ExLogger
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="BillNo"></param>
        /// <param name="BillDate"></param>
        /// <param name="StoreCode"></param>
        /// <param name="ErrorMessage"></param>
        /// <param name="ErrorDiscription"></param>
        /// <param name="ConString"></param>
        public static void ExLogger(int TransactionID, string BillNo, string BillDate, string StoreCode, string ErrorMessage, string ErrorDiscription, string ConString)
        {
            try
            {
                DataTable dt = new DataTable();
                IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
                var constr = config.GetSection("ConnectionStrings").GetSection("HomeShop").Value;
                MySqlConnection con = new MySqlConnection(ConString);
                MySqlCommand cmd = new MySqlCommand("SP_PHYInsertErrorLog", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@_transactionID", TransactionID);
                cmd.Parameters.AddWithValue("@_billNo", BillNo);
                cmd.Parameters.AddWithValue("@_billDate", BillDate);
                cmd.Parameters.AddWithValue("@_storeCode", StoreCode);
                cmd.Parameters.AddWithValue("@_errorMessage", ErrorMessage);
                cmd.Parameters.AddWithValue("@_errorDiscription", ErrorDiscription);
                cmd.Parameters.AddWithValue("@_repeatCount", 0);
                cmd.Parameters.AddWithValue("@_functionName", "Payment Status");
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                
            }
            finally { GC.Collect(); }
        }

    }
}
