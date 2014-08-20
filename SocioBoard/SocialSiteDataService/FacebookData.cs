﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocioBoard.Helper;
using Facebook;
using SocioBoard.Model;
using System.Collections;
using SocioBoard.Domain;
using System.Net;
using System.IO;

namespace SocialSiteDataService
{
    class FacebookData:SocialSiteDataFeeds
    {
        public void GetFacebookData(object userId)
        {
            Guid UserId = (Guid)userId;
            FacebookAccountRepository objFbRepo = new FacebookAccountRepository();
            FacebookHelper fbhelper = new FacebookHelper();
            ArrayList arrFbAcc = objFbRepo.getAllFacebookAccountsOfUser(UserId);
            foreach (FacebookAccount itemFb in arrFbAcc)
            {
                 FacebookHelper objFbHelper = new FacebookHelper();
                 FacebookInsightStatsHelper fbiHelper = new FacebookInsightStatsHelper();
                 try
                 {                  
                    FacebookClient fb = new FacebookClient();
                    fb.AccessToken = itemFb.AccessToken;

                    var feeds = fb.Get("/me/feed");
                    var home = fb.Get("me/home");
                    var profile = fb.Get("me");
                    getFacebookUserHome(home, profile, UserId);
                    getFacebookUserFeeds(feeds, profile, UserId);
                    getFacebookProfile(profile, UserId);
                    FacebookAccountRepository fbAccRepo = new FacebookAccountRepository();
                    try
                    {
                        int fancountacc = 0;
                        dynamic fanacccount = fb.Get("fql", new { q = "SELECT friend_count FROM user WHERE uid=" + itemFb.FbUserId });
                        foreach (var friend in fanacccount.data)
                        {
                            fancountacc = Convert.ToInt32(friend.friend_count);
                        }
                       
                        fbAccRepo.updateFriendsCount(itemFb.FbUserId, itemFb.UserId, fancountacc);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    if (itemFb.Type == "page")
                    {
                        try
                        {
                            fbiHelper.getFanPageLikesByGenderAge(itemFb.FbUserId, UserId, 10);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        try
                        {
                            fbiHelper.getPageImpresion(itemFb.FbUserId, UserId, 10);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                                
                        }
                        try
                        {
                            fbiHelper.getStories(itemFb.FbUserId, UserId, 10);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        try
                        {
                            fbiHelper.getLocation(itemFb.FbUserId, UserId, 10);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        try
                        {
                            fbiHelper.getFanPost(itemFb.FbUserId, UserId, 10);
                        }
                        catch (Exception ex)
                        {
                        }

                        try
                        {
                            int fancountPage = 0;
                            dynamic fancount = fb.Get("fql", new { q = " SELECT fan_count FROM page WHERE page_id =" + itemFb.FbUserId });
                            foreach (var friend in fancount.data)
                            {
                                fancountPage = Convert.ToInt32(friend.fan_count);
                            }
                           // FacebookAccountRepository fbAccRepo = new FacebookAccountRepository();
                            fbAccRepo.updateFansCount(itemFb.FbUserId, itemFb.UserId, fancountPage);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                    }
                  //  var friendsgenderstats = fb.Get("me/friends?fields=gender");
                   // fbhelper.getfbFriendsGenderStats(friendsgenderstats, profile, UserId);
                }
                catch (Exception Err)
                {
                    Console.Write(Err.Message);
                }
            }
        }



        public void getFacebookUserHome(dynamic data, dynamic profile, Guid userId)
        {
            FacebookMessage fbmsg = new FacebookMessage();
            FacebookMessageRepository fbmsgrepo = new FacebookMessageRepository();
            int lstfbcount = 0;
            foreach (dynamic result in data["data"])
            {

                string message = string.Empty;
                string imgprof = "http://graph.facebook.com/" + result["from"]["id"] + "/picture?type=small";
                fbmsg.EntryDate = DateTime.Now;
                fbmsg.MessageId = result["id"].ToString();
                fbmsg.FromId = result["from"]["id"].ToString();
                fbmsg.FromName = result["from"]["name"].ToString();
                fbmsg.FromProfileUrl = imgprof;
                fbmsg.Id = Guid.NewGuid();
                fbmsg.MessageDate = DateTime.Parse(result["created_time"].ToString());
                fbmsg.UserId = userId;
                fbmsg.Type = "fb_home";
                fbmsg.ProfileId = profile["id"].ToString();
                fbmsg.FbComment = "http://graph.facebook.com/" + result["id"] + "/comments";
                fbmsg.FbLike = "http://graph.facebook.com/" + result["id"] + "/likes";


                if (lstfbcount < 25)
                {
                    try
                    {
                        if (result["message"] != null)
                        {
                            message = result["message"];
                            lstfbcount++;

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                        try
                        {
                            if (result["description"] != null)
                            {
                                message = result["description"];
                                lstfbcount++;

                            }
                        }
                        catch (Exception exx)
                        {
                            try
                            {
                                Console.WriteLine(exx.StackTrace);
                                if (result["story"] != null)
                                {
                                    message = result["story"];
                                    lstfbcount++;
                                }
                            }
                            catch (Exception exxx)
                            {
                                Console.WriteLine(exxx.StackTrace);
                                message = string.Empty;
                            }
                        }

                    }

                }
                fbmsg.Message = message;

                if (!fbmsgrepo.checkFacebookMessageExists(fbmsg.MessageId, fbmsg.UserId))
                {
                    fbmsgrepo.addFacebookMessage(fbmsg);
                }
            }
        }

        public void getFacebookUserFeeds(dynamic data, dynamic profile, Guid userId)
        {
            FacebookFeed fbfeed = new FacebookFeed();
            FacebookFeedRepository fbfeedrepo = new FacebookFeedRepository();

            if (data != null)
            {
                foreach (var result in data["data"])
                {

                    fbfeed.Type = "fb_feed";
                    try
                    {
                        fbfeed.UserId = userId;
                    }
                    catch (Exception ex)
                    { Console.WriteLine(ex.StackTrace); }

                    try
                    {
                        fbfeed.ProfileId = profile["id"].ToString();
                    }
                    catch (Exception ex)
                    { Console.WriteLine(ex.StackTrace); }

                    try
                    {
                        fbfeed.Id = Guid.NewGuid();
                    }
                    catch (Exception ex)
                    { Console.WriteLine(ex.StackTrace); }



                    fbfeed.FromProfileUrl = "http://graph.facebook.com/" + result["from"]["id"] + "/picture?type=small";
                    fbfeed.FromName = result["from"]["name"].ToString();
                    fbfeed.FromId = result["from"]["id"].ToString();
                    fbfeed.FeedId = result["id"].ToString();
                    fbfeed.FeedDate = DateTime.Parse(result["created_time"].ToString());
                    fbfeed.FbComment = "http://graph.facebook.com/" + result["id"] + "/comments";
                    fbfeed.FbLike = "http://graph.facebook.com/" + result["id"] + "/likes";
                    string message = string.Empty;
                    int lstfbcount = 0;

                    if (lstfbcount < 25)
                    {
                        try
                        {
                            if (result["message"] != null)
                            {
                                message = result["message"];
                                lstfbcount++;

                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.StackTrace);
                            try
                            {
                                if (result["description"] != null)
                                {
                                    message = result["description"];
                                    lstfbcount++;

                                }
                            }
                            catch (Exception exx)
                            {
                                try
                                {
                                    Console.WriteLine(exx.StackTrace);
                                    if (result["story"] != null)
                                    {
                                        message = result["story"];
                                        lstfbcount++;
                                    }
                                }
                                catch (Exception exxx)
                                {
                                    Console.WriteLine(exxx.StackTrace);
                                    message = string.Empty;
                                }
                            }

                        }
                    }
                    fbfeed.FeedDescription = message;
                    fbfeed.EntryDate = DateTime.Now;

                    if (!fbfeedrepo.checkFacebookFeedExists(fbfeed.FeedId, userId))
                    {
                        fbfeedrepo.addFacebookFeed(fbfeed);
                    }
                }
            }
        }

        public void getFacebookProfile(dynamic data,Guid user)
        { 
        
        }



        public void GetData(object userId)
        {
            Guid UserId = (Guid)userId;
            FacebookAccountRepository objFbRepo = new FacebookAccountRepository();
            FacebookHelper fbhelper = new FacebookHelper();
            ArrayList arrFbAcc = objFbRepo.getAllFacebookAccountsOfUser(UserId);
            foreach (FacebookAccount itemFb in arrFbAcc)
            {
                FacebookHelper objFbHelper = new FacebookHelper();
                FacebookInsightStatsHelper fbiHelper = new FacebookInsightStatsHelper();
                try
                {
                    FacebookClient fb = new FacebookClient();
                    fb.AccessToken = itemFb.AccessToken;

                    var feeds = fb.Get("/me/feed");
                    var home = fb.Get("me/home");
                    var profile = fb.Get("me");
                    getFacebookUserHome(home, profile, UserId);
                    getFacebookUserFeeds(feeds, profile, UserId);
                    getFacebookProfile(profile, UserId);
                    FacebookAccountRepository fbAccRepo = new FacebookAccountRepository();
                    try
                    {
                        int fancountacc = 0;
                        dynamic fanacccount = fb.Get("fql", new { q = "SELECT friend_count FROM user WHERE uid=" + itemFb.FbUserId });
                        foreach (var friend in fanacccount.data)
                        {
                            fancountacc = Convert.ToInt32(friend.friend_count);
                        }

                        fbAccRepo.updateFriendsCount(itemFb.FbUserId, itemFb.UserId, fancountacc);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    if (itemFb.Type == "page")
                    {
                        try
                        {
                            fbiHelper.getFanPageLikesByGenderAge(itemFb.FbUserId, UserId, 10);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        try
                        {
                            fbiHelper.getPageImpresion(itemFb.FbUserId, UserId, 10);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);

                        }
                        try
                        {
                            fbiHelper.getStories(itemFb.FbUserId, UserId, 10);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        try
                        {
                            fbiHelper.getLocation(itemFb.FbUserId, UserId, 10);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        try
                        {
                            fbiHelper.getFanPost(itemFb.FbUserId, UserId, 10);
                        }
                        catch (Exception ex)
                        {
                        }

                        try
                        {
                            int fancountPage = 0;
                            dynamic fancount = fb.Get("fql", new { q = " SELECT fan_count FROM page WHERE page_id =" + itemFb.FbUserId });
                            foreach (var friend in fancount.data)
                            {
                                fancountPage = Convert.ToInt32(friend.fan_count);
                            }
                            // FacebookAccountRepository fbAccRepo = new FacebookAccountRepository();
                            fbAccRepo.updateFansCount(itemFb.FbUserId, itemFb.UserId, fancountPage);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                    }
                    //  var friendsgenderstats = fb.Get("me/friends?fields=gender");
                    // fbhelper.getfbFriendsGenderStats(friendsgenderstats, profile, UserId);
                }
                catch (Exception Err)
                {
                    Console.Write(Err.Message);
                }
            }
        }

        public void GetSearchData(object parameters)
        {
            #region Facebook
            try
            {
                Array arrayParams = (Array)parameters;

                DiscoverySearch dissearch = (DiscoverySearch)arrayParams.GetValue(0);
                DiscoverySearchRepository dissearchrepo = (DiscoverySearchRepository)arrayParams.GetValue(1);
                DiscoverySearch discoverySearch = (DiscoverySearch)arrayParams.GetValue(2);


                #region FacebookSearch

                string accesstoken = string.Empty;

                FacebookAccountRepository fbAccRepo = new FacebookAccountRepository();
                //ArrayList asltFbAccount = fbAccRepo.getAllFacebookAccounts();
                ArrayList asltFbAccount = fbAccRepo.getAllFacebookAccountsOfUser(discoverySearch.UserId);

                foreach (FacebookAccount item in asltFbAccount)
                {
                    accesstoken = item.AccessToken;
                    if (FacebookHelper.CheckFacebookToken(accesstoken, discoverySearch.SearchKeyword))
                    {

                        break;
                    }
                }

                string facebookSearchUrl = "https://graph.facebook.com/search?q=" + discoverySearch.SearchKeyword + " &type=post&access_token=" + accesstoken;
                var facerequest = (HttpWebRequest)WebRequest.Create(facebookSearchUrl);
                facerequest.Method = "GET";
                string outputface = string.Empty;
                using (var response = facerequest.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
                    {
                        outputface = stream.ReadToEnd();
                    }
                }
                if (!outputface.StartsWith("["))
                    outputface = "[" + outputface + "]";


                Newtonsoft.Json.Linq.JArray facebookSearchResult = Newtonsoft.Json.Linq.JArray.Parse(outputface);

                foreach (var item in facebookSearchResult)
                {
                    var data = item["data"];

                    foreach (var chile in data)
                    {
                        try
                        {
                            dissearch.CreatedTime = DateTime.Parse(chile["created_time"].ToString());

                            dissearch.EntryDate = DateTime.Now;

                            dissearch.FromId = chile["from"]["id"].ToString();

                            dissearch.FromName = chile["from"]["name"].ToString();

                            try
                            {
                                dissearch.ProfileImageUrl = "http://graph.facebook.com/" + chile["from"]["id"] + "/picture?type=small";
                            }
                            catch { }

                            dissearch.SearchKeyword = discoverySearch.SearchKeyword;

                            dissearch.Network = "facebook";

                            try
                            {
                                dissearch.Message = chile["message"].ToString();
                            }
                            catch { }
                            try
                            {
                                dissearch.Message = chile["story"].ToString();
                            }
                            catch { }

                            dissearch.MessageId = chile["id"].ToString();

                            dissearch.Id = Guid.NewGuid();

                            dissearch.UserId = discoverySearch.UserId;

                            if (!dissearchrepo.isKeywordPresent(dissearch.SearchKeyword, dissearch.MessageId))
                            {
                                dissearchrepo.addNewSearchResult(dissearch);
                            }
                        }
                        catch (Exception ex)
                        {
                            //logger.Error(ex.StackTrace);
                            Console.WriteLine(ex.StackTrace);
                        }


                    }
                }


            }
            catch (Exception ex)
            {
                //logger.Error(ex.StackTrace);
                Console.WriteLine(ex.StackTrace);
            }
                #endregion

            #endregion
        }



       
    }
}