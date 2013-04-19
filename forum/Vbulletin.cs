﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Forms;
using POG.Utils;
using Newtonsoft;
using System.Diagnostics;
using System.Xml;
using System.Web;

namespace POG.Forum
{
    internal class VBulletin_4_2_0 : VBulletinSM
    {
        internal VBulletin_4_2_0(VBulletinForum outer, StateMachineHost host, String forum, String lobby, Action<Action> synchronousInvoker) :
            base(outer, host, forum, lobby, synchronousInvoker)
        {
        }
        internal override ThreadReader Reader()
        {
            ThreadReader t = new ThreadReader_4_2_0(_connectionSettings, _synchronousInvoker);
            return t;
        }
        internal override LobbyReader Lobby()
        {
            LobbyReader lr = new LobbyReaderEstonia(_connectionSettings, _synchronousInvoker);
            return lr;
        }
        protected override String GetOurUserId(ConnectionSettings cs)
        {
            String rc = cs.CC.GetCookies(new System.Uri(ForumURL))["bb_userid"].Value;
            return rc;
        }
    }
    internal class VBulletin_3_8_7 : VBulletinSM
    {
        internal VBulletin_3_8_7(VBulletinForum outer, StateMachineHost host, String forum, String lobby, Action<Action> synchronousInvoker) :
            base(outer, host, forum, lobby, synchronousInvoker)
        {
        }
    }
    internal class VBulletinSM : StateMachine
	{
		#region members
		VBulletinForum _outer;
		protected ConnectionSettings _connectionSettings;
		protected Action<Action> _synchronousInvoker;
		private String _username = ""; // if this changes from what user entered previously, need to logout then re-login with new info.
		int _postsPerPage = 50;

		#endregion

		internal VBulletinSM(VBulletinForum outer, StateMachineHost host, String forum, String lobby, Action<Action> synchronousInvoker) :
			base("VBulletin", host)
		{
			_outer = outer;
			_synchronousInvoker = synchronousInvoker;
            ForumHost = forum;
            ForumLobby = lobby;
            _connectionSettings = new ConnectionSettings(ForumURL);
			SetInitialState(StateLoggedOut);
		}
		#region Properties
		internal virtual ThreadReader Reader()
		{
			ThreadReader t = new ThreadReader(_connectionSettings, _synchronousInvoker);
			return t;
		}
		internal virtual LobbyReader Lobby()
		{
			LobbyReader lr = new LobbyReader(_connectionSettings, _synchronousInvoker);
			return lr;
		}

		public Int32 PostsPerPage {
			get
			{
				return _postsPerPage;
			}
		}
		#endregion
		#region States
		private State StateTop(Event e)
		{
			return null;
		}
		private State StateLoggedOut(Event e)
		{
			switch (e.EventName)
			{
				case "EventEnter":
					{
						if (_username != null)
						{
							_outer.OnStatusUpdate("Logging out " + _username);
							DoLogout();
							_outer.OnStatusUpdate("Logged out " + _username);
							_outer.OnLoginEvent(_username, LoginEventType.LogoutSuccess);
						}
					}
					return null;

				case "DoLogin":
					{
						Event<String, String> evtLogin = e as Event<String, String>;
						String resp = DoLogin(evtLogin.Param1, evtLogin.Param2);
						if (resp != null)
						{
							_outer.OnStatusUpdate(resp);
							_outer.OnLoginEvent(_username, LoginEventType.LoginFailure);
						}
						else
						{
							_outer.OnStatusUpdate("Successful login as " + _username);
							_outer.OnLoginEvent(_username, LoginEventType.LoginSuccess);
							ChangeState(StateLoggedIn);
						}

					}
					return null;
			}
			return StateTop;
		}
		private State StateLoggedIn(Event e)
		{
			switch (e.EventName)
			{
				case "EventEnter":
					{
					}
					return null;

				case "EventExit":
					{
					}
					return null;

				case "DoLogout":
					{
						ChangeState(StateLoggedOut);
					}
					return null;

				case "GetPostersLike":
					{
						Event<String, Action<String, IEnumerable<Poster>>> evt = 
							e as Event<String, Action<String, IEnumerable<Poster>>>;
						DoGetPostersLike(evt.Param1, evt.Param2);
					}
					return null;
			}
			return StateTop;
		}
		#endregion
		#region private methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="username">username of gimmick account</param>
		/// <param name="password">password of gimmick account</param>
		/// <returns></returns>
		private String DoLogin(String username, String password)
		{
			// Get some needed cookies!
			_connectionSettings.Url = ForumURL;
			String page = HtmlHelper.GetUrlResponseString(_connectionSettings);
			if (page == null)
			{
				return "Error loggin in. Could not reach " + ForumURL;
			}
			// They are hidden in javascript...
			Regex reg = new Regex(Regex.Escape("setCookie('") + "([^\']*)\', \'([^\']*)\'");
			Match match = reg.Match(page);
			if (match.Success)
			{
				String host = ForumHost;
				string cookieName = match.Groups[1].Value;
				string cookieValue = match.Groups[2].Value;
				Cookie cookie = new Cookie(cookieName, cookieValue, "/", host);
				_connectionSettings.CC.Add(cookie);
				Cookie cReferrer = new Cookie("DOAReferrer", ForumURL, "/", host);
				_connectionSettings.CC.Add(cReferrer);
			}
            Cookie cLanguage = new Cookie("bb_languageid", "2", "/", ForumHost); // Estonian!
            _connectionSettings.CC.Add(cLanguage);
			_username = null;
			String hashedPassword = SecurityUtils.md5(password);

			_connectionSettings.Url = String.Format("{0}login.php?do=login", ForumURL);
			_connectionSettings.Data =
				String.Format("vb_login_username={0}&cookieuser=1&vb_login_password=&s=&securitytoken=guest&do=login&vb_login_md5password={1}&vb_login_md5password_utf={1}", username, hashedPassword);
			String resp = HtmlHelper.PostToUrl(_connectionSettings);

			if (resp == null)
			{
				// login failure
				return "The following error occurred while logging in:\n\n" + _connectionSettings.Message;
			}

			if (!resp.Contains("exec_refresh()"))
			{
				return "Error logging in.  Please verify login information.";
			}


			// set posts/page
			_connectionSettings.Url = String.Format("{0}profile.php?do=editoptions", ForumURL);
			_username = username;
			resp = HtmlHelper.GetUrlResponseString(_connectionSettings);
			if (resp != null)
			{
				Match m = Regex.Match(resp, "umaxposts.*?value=\"(-?\\d+)\"[ ](class=\"[A-z0-9]*\")?[ ]*selected=\"selected\"", RegexOptions.Singleline);
				if (m.Success)
				{
					Int32 postsPerPage = Int32.Parse(m.Groups[1].Value);
					if (postsPerPage == -1)
					{
						_postsPerPage = 15;
					}
					else
					{
						_postsPerPage = postsPerPage;
					}
				}
				else
				{

					_postsPerPage = 100;
					return
						"Login success, but there was an error setting the posts/page.  Please set your settings to 100 posts/page.";
				}

			}
			else
			{
				_postsPerPage = 100;
				return
					"Login success, but there was an error setting the posts/page.  Please set your settings to 100 posts/page.";
			}

			// logging in sets the cookies in connectionSettings so we don't have to do anything else.
			return null;
		}

		private void DoLogout()
		{
			// get the page once to find the logout url
			_connectionSettings.Url = ForumURL;
			String resp = HtmlHelper.GetUrlResponseString(_connectionSettings);
			if (resp != null)
			{
				Match m = Regex.Match(resp, "logouthash=([A-z0-9-])");
				if (m.Success)
				{
					String hash = m.Groups[1].Value;
					_connectionSettings.Url = String.Format("login.php?do=logout&amp;logouthash={0}", ForumURL, hash);
					HtmlHelper.GetUrlResponseString(_connectionSettings);
					_connectionSettings.CC = new CookieContainer(); // just in case
					_username = null;
				}
			}
		}
		internal void Login(string username, string password)
		{
			PostEvent(new Event<string, string>("DoLogin", username, password));
		}
		internal void Logout()
		{
			PostEvent(new Event("DoLogout"));
		}
		internal void GetAlias()
		{
			ConnectionSettings cs = _connectionSettings.Clone();
			cs.Url = "http://www.checkmywwstats.com/.hidden/ajax/lookupalias.php";
			cs.Data = @"data={""votes"":[""Chips""],""playerlist"":[""aksdal"", ""CPHoya"", ""Larry Legend"", ""Chips Ahoy"", ""Thingyman""]}";
			// "submitter=oreos"
			//cs.Data = "x=oreos";
			cs.Message = cs.Data;
			//Trace.TraceInformation("Posting: " + cs.Data);
			String resp = HtmlHelper.PostToUrl(cs);
			if (resp == null)
			{
				// failure
			}

		}
		#endregion
		internal Boolean MakePost(Int32 threadId, String title, String content, Boolean lockit, Int32 icon)
		{
			Boolean rc = DoMakePost(threadId, title, content, lockit, icon);
			return rc;
		}
        internal bool CanUserReceivePM(string name)
        {
            ConnectionSettings cs = _connectionSettings.Clone();
            cs.Url = _outer.ForumURL + "private.php";
            String doc = HtmlHelper.GetUrlResponseString(cs);
            if (doc == null)
            {
                return false;
            }
            // parse out securitytoken
            Regex reg = new Regex("var SECURITYTOKEN = \"(.+)\"");
            String securityToken = "";
            Match match = reg.Match(doc);
            if (match.Success)
            {
                securityToken = match.Groups[1].Value;
            }
            //			var ajax_last_post = 1338251071;
            String ajaxLastPost = "";
            reg = new Regex("var ajax_last_post = (.+);");
            match = reg.Match(doc);
            if (match.Success)
            {
                ajaxLastPost = match.Groups[1].Value;
            }
            StringBuilder msg = new StringBuilder();

            String sTo = name;
            
            String sBCC = String.Empty;
            msg.AppendFormat("{0}={1}&", "recipients", sTo);
            msg.AppendFormat("{0}={1}&", "bccrecipients", sBCC);
            String title = "test";
            if (title != String.Empty)
            {
                msg.AppendFormat("{0}={1}&", "title", HttpUtility.UrlEncode(title));
            }
            String content = "test";
            content = content.Replace("\r\n", "\n");
            msg.AppendFormat("{0}={1}&", "message", HttpUtility.UrlEncode(content));
            msg.AppendFormat("{0}={1}&", "wysiwyg", "0");
            msg.AppendFormat("{0}={1}&", "iconid", "0");
            msg.AppendFormat("{0}={1}&", "s", "");
            msg.AppendFormat("{0}={1}&", "securitytoken", securityToken);
            msg.AppendFormat("{0}={1}&", "do", "insertpm");
            msg.AppendFormat("{0}={1}&", "pmid", "");
            msg.AppendFormat("{0}={1}&", "forward", "");
            msg.AppendFormat("{0}={1}&", "preview", "Preview Message");
            msg.AppendFormat("{0}={1}&", "savecopy", "1");
            msg.AppendFormat("{0}={1}&", "parseurl", "1");

            cs.Url = String.Format("{0}private.php?do=insertpm&pmid=", _outer.ForumURL);
            cs.Data = msg.ToString();
            //Trace.TraceInformation("Posting: " + cs.Data);
            String resp = HtmlHelper.PostToUrl(cs);
            if (resp == null)
            {
                // failure
                return false;
            }
            // Parse out response.
            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(resp);
            HtmlAgilityPack.HtmlNode root = html.DocumentNode;
            if (root == null)
            {
                return false;
            }
            HtmlAgilityPack.HtmlNode status = root.SelectSingleNode("/html[1]/body[1]/table[2]/tr[2]/td[1]/td[1]/div[1]/div[1]/div[1]/table[2]/tr[1]/td[3]/table[1]/tr[1]/td");
            if (status != null)
            {
                String result = status.InnerText.Trim();
                // Error looks like "The following errors occurred with your submission:"
                if (result.Contains("Preview"))
                {
                    return true;
                }
            }
            return false;
        }

        internal Boolean SendPM(IEnumerable<string> To, IEnumerable<string> bcc, string title, string content, bool receipt)
        {
            ConnectionSettings cs = _connectionSettings.Clone();
            cs.Url = _outer.ForumURL + "private.php";
            String doc = HtmlHelper.GetUrlResponseString(cs);
            if (doc == null)
            {
                return false;
            }
            // parse out securitytoken
            Regex reg = new Regex("var SECURITYTOKEN = \"(.+)\"");
            String securityToken = "";
            Match match = reg.Match(doc);
            if (match.Success)
            {
                securityToken = match.Groups[1].Value;
            }
            //			var ajax_last_post = 1338251071;
            String ajaxLastPost = "";
            reg = new Regex("var ajax_last_post = (.+);");
            match = reg.Match(doc);
            if (match.Success)
            {
                ajaxLastPost = match.Groups[1].Value;
            }
            /*				<input type="hidden" name="fromquickreply" value="1" />
                <input type="hidden" name="s" value="" />
                <input type="hidden" name="securitytoken" value="1338251187-cd0e85748ac090ba7cb5281011b49332c0ba455a" />
                <input type="hidden" name="do" value="postreply" />
                <input type="hidden" name="t" value="1204368" id="qr_threadid" />
                <input type="hidden" name="p" value="who cares" id="qr_postid" />
                <input type="hidden" name="specifiedpost" value="0" id="qr_specifiedpost" />
                <input type="hidden" name="parseurl" value="1" />
                <input type="hidden" name="loggedinuser" value="198669" />
             * 
             * openclose=1
             * open:
             * POST http://forumserver.twoplustwo.com/postings.php?t=1204368&pollid= 
             * do=openclosethread&s=&securitytoken=1338253787-f7a244e5e823e6d88002169f4f314e97febf4dce&t=1204368&pollid=
             * close:
             * POST /postings.php?t=1204368&pollid= HTTP/1.1
             * do=openclosethread&s=&securitytoken=1338253981-5657f92aed9e901b4292a60cab0e9efaac4be1fe&t=1204368&pollid=
*/

            StringBuilder msg = new StringBuilder();

            String sTo = String.Empty;
            if (To != null)
            {
                sTo = String.Join("; ", To);
            }
            String sBCC = String.Empty;
            if (bcc != null)
            {
                sBCC = String.Join("; ", bcc);
            }
            msg.AppendFormat("{0}={1}&", "recipients", sTo);
            msg.AppendFormat("{0}={1}&", "bccrecipients", sBCC);
            if (title != String.Empty)
            {
                msg.AppendFormat("{0}={1}&", "title", HttpUtility.UrlEncode(title));
            }
            content = content.Replace("\r\n", "\n");
            msg.AppendFormat("{0}={1}&", "message", HttpUtility.UrlEncode(content));
            msg.AppendFormat("{0}={1}&", "wysiwyg", "0");
            msg.AppendFormat("{0}={1}&", "iconid", "0");
            msg.AppendFormat("{0}={1}&", "s", "");
            msg.AppendFormat("{0}={1}&", "securitytoken", securityToken);
            msg.AppendFormat("{0}={1}&", "do", "insertpm");
            msg.AppendFormat("{0}={1}&", "pmid", "");
            msg.AppendFormat("{0}={1}&", "forward", "");
            msg.AppendFormat("{0}={1}&", "sbutton", "Submit Button");
            msg.AppendFormat("{0}={1}&", "receipt", "1");
            msg.AppendFormat("{0}={1}&", "savecopy", "1");
            msg.AppendFormat("{0}={1}&", "parseurl", "1");

            cs.Url = String.Format("{0}private.php?do=insertpm&pmid=", _outer.ForumURL);
            cs.Data = msg.ToString();
            //Trace.TraceInformation("Posting: " + cs.Data);
            String resp = HtmlHelper.PostToUrl(cs);
            if (resp == null)
            {
                // failure
                return false;
            }

            return true;
        }
        internal bool DeleteThread(int postId)
        {
            ConnectionSettings cs = _connectionSettings.Clone();
            cs.Url = _outer.ForumURL + "private.php";
            String doc = HtmlHelper.GetUrlResponseString(cs);
            if (doc == null)
            {
                return false;
            }
            // parse out securitytoken
            Regex reg = new Regex("var SECURITYTOKEN = \"(.+)\"");
            String securityToken = "";
            Match match = reg.Match(doc);
            if (match.Success)
            {
                securityToken = match.Groups[1].Value;
            }
            //			var ajax_last_post = 1338251071;
            String ajaxLastPost = "";
            reg = new Regex("var ajax_last_post = (.+);");
            match = reg.Match(doc);
            if (match.Success)
            {
                ajaxLastPost = match.Groups[1].Value;
            }
            StringBuilder msg = new StringBuilder();

            msg.AppendFormat("{0}={1}&", "do", "deletepost");
            msg.AppendFormat("{0}={1}&", "s", "");
            msg.AppendFormat("{0}={1}&", "securitytoken", securityToken);
            msg.AppendFormat("{0}={1}&", "postid", postId.ToString());
            msg.AppendFormat("{0}={1}&", "deletepost", "delete");
            msg.AppendFormat("{0}={1}&", "reason", "");
            cs.Url = String.Format("{0}editpost.php", _outer.ForumURL);
            cs.Data = msg.ToString();
            //Trace.TraceInformation("Posting: " + cs.Data);
            String resp = HtmlHelper.PostToUrl(cs);
            if (resp == null)
            {
                // failure
                return false;
            }
            return true;
        }
        internal String NewThread(Int32 forum, String title, String body, Int32 icon, Boolean lockit)
        {
            String rc = String.Empty;
            ConnectionSettings cs = _connectionSettings.Clone();
            cs.Url = _outer.ForumURL + "private.php";
            String doc = HtmlHelper.GetUrlResponseString(cs);
            if (doc == null)
            {
                return rc;
            }
            // parse out securitytoken
            Regex reg = new Regex("var SECURITYTOKEN = \"(.+)\"");
            String securityToken = "";
            Match match = reg.Match(doc);
            if (match.Success)
            {
                securityToken = match.Groups[1].Value;
            }
            //			var ajax_last_post = 1338251071;
            String ajaxLastPost = "";
            reg = new Regex("var ajax_last_post = (.+);");
            match = reg.Match(doc);
            if (match.Success)
            {
                ajaxLastPost = match.Groups[1].Value;
            }
            StringBuilder msg = new StringBuilder();

            msg.AppendFormat("{0}={1}&", "subject", title);
            msg.AppendFormat("{0}={1}&", "message", body);
            msg.AppendFormat("{0}={1}&", "wysiwyg", "0");
            msg.AppendFormat("{0}={1}&", "iconid", icon.ToString());
            msg.AppendFormat("{0}={1}&", "s", "");
            msg.AppendFormat("{0}={1}&", "securitytoken", securityToken);
            msg.AppendFormat("{0}={1}&", "f", forum.ToString());
            msg.AppendFormat("{0}={1}&", "do", "postthread");
            msg.AppendFormat("{0}={1}&", "posthash", String.Empty);
            msg.AppendFormat("{0}={1}&", "poststarttime", String.Empty);
            if (lockit)
            {
                msg.AppendFormat("{0}={1}&", "openclose", "1");
            }
            msg.AppendFormat("{0}={1}", "loggedinuser", GetOurUserId(cs));
            msg.AppendFormat("{0}={1}&", "sbutton", "Submit New Thread");
            msg.AppendFormat("{0}={1}&", "parseurl", "1");
            msg.AppendFormat("{0}={1}&", "emailupdate", "0");
            msg.AppendFormat("{0}={1}&", "polloptions", "4");
            cs.Url = String.Format("{0}newthread.php?do=postthread&f=", _outer.ForumURL, forum);
            cs.Data = msg.ToString();
            //Trace.TraceInformation("Posting: " + cs.Data);
            String resp = HtmlHelper.PostToUrl(cs);
            if (resp == null)
            {
                // failure
                return rc;
            }
            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml(resp);
            HtmlAgilityPack.HtmlNode root = html.DocumentNode;
            HtmlAgilityPack.HtmlNode link = root.SelectSingleNode("html/head/link[@rel='canonical']");
            if (link != null)
            {
                String url = link.Attributes["href"].Value;
                rc = url;
            }
            return rc;
        }
        Boolean DoMakePost(Int32 threadId, String title, String content, Boolean lockit, Int32 icon)
		{
			/* headers
				POST /newreply.php?do=postreply&t=1198532 HTTP/1.1
				Host: forumserver.twoplustwo.com
				Connection: keep-alive
				Content-Length: 299
				Origin: http://forumserver.twoplustwo.com
				X-Requested-With: XMLHttpRequest
				User-Agent: Mozilla/5.0 (Windows NT 5.1) AppleWebKit/536.5 (KHTML, like Gecko) Chrome/19.0.1084.52 Safari/536.5
				Content-Type: application/x-www-form-urlencoded; charset=UTF-8
				Accept: * /* <-- space added
				Referer: http://forumserver.twoplustwo.com/59/puzzles-other-games/pog-pub-may-2012-lc-now-even-more-gimmicks-nsfw-1198532/index62.html
				Accept-Encoding: gzip,deflate,sdch
				Accept-Language: en-US,en;q=0.8
				Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.3
				Cookie: __utmz=126502536.1325477731.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); Listing15=Open; Listing8=Open; Listing2=Open; bblastvisit=1325477737; bblastactivity=0; bbuserid=81718; bbpassword=bed5d97301ccb4e84503ce49bedcf76a; __utma=126502536.1375902440.1325477731.1337991697.1338028399.110; __utmc=126502536; bbsessionhash=2c6af250e2d1881d045bd64c74a8626f; vbseo_loggedin=yes; Listing9=Open; __utmb=126502536

			 * querystring do=postreply&t=1198532
			 * body --
securitytoken blah
ajax 1
ajax_lastpost 1338033352
message test
wysiwyg 0 
styleid 0
fromquickreply 1
s
securitytoken blah
do postreply
t 1198532
p who care
specifiedpost 0
parseurl 1
loggedinuser 81788
			 * openclose 1 <-- lock thread
			 * */
			ConnectionSettings cs = _connectionSettings.Clone();
			cs.Url = ForumURL + "private.php";
			String doc = HtmlHelper.GetUrlResponseString(cs);
			if (doc == null)
			{
				return false;
			}
			// parse out securitytoken
			Regex reg = new Regex("var SECURITYTOKEN = \"(.+)\"");
			String securityToken = "";
			Match match = reg.Match(doc);
			if (match.Success)
			{
				securityToken = match.Groups[1].Value;
			}
			//			var ajax_last_post = 1338251071;
			String ajaxLastPost = "";
			reg = new Regex("var ajax_last_post = (.+);");
			match = reg.Match(doc);
			if (match.Success)
			{
				ajaxLastPost = match.Groups[1].Value;
			}
			/*				<input type="hidden" name="fromquickreply" value="1" />
				<input type="hidden" name="s" value="" />
				<input type="hidden" name="securitytoken" value="1338251187-cd0e85748ac090ba7cb5281011b49332c0ba455a" />
				<input type="hidden" name="do" value="postreply" />
				<input type="hidden" name="t" value="1204368" id="qr_threadid" />
				<input type="hidden" name="p" value="who cares" id="qr_postid" />
				<input type="hidden" name="specifiedpost" value="0" id="qr_specifiedpost" />
				<input type="hidden" name="parseurl" value="1" />
				<input type="hidden" name="loggedinuser" value="198669" />
			 * 
			 * openclose=1
			 * open:
			 * POST http://forumserver.twoplustwo.com/postings.php?t=1204368&pollid= 
			 * do=openclosethread&s=&securitytoken=1338253787-f7a244e5e823e6d88002169f4f314e97febf4dce&t=1204368&pollid=
			 * close:
			 * POST /postings.php?t=1204368&pollid= HTTP/1.1
			 * do=openclosethread&s=&securitytoken=1338253981-5657f92aed9e901b4292a60cab0e9efaac4be1fe&t=1204368&pollid=
*/
			StringBuilder msg = new StringBuilder();
			
			msg.AppendFormat("{0}={1}&", "securitytoken", securityToken);
			content = content.Replace("\r\n", "\n");
			if (title != String.Empty)
			{
				msg.AppendFormat("{0}={1}&", "title", HttpUtility.UrlEncodeUnicode(title));
			}
            msg.AppendFormat("{0}={1}&", "ajax", "1");
            msg.AppendFormat("{0}={1}&", "ajax_lastpost", "1");
            msg.AppendFormat("{0}={1}&", "message_backup", HttpUtility.UrlEncodeUnicode(content));
            msg.AppendFormat("{0}={1}&", "message", HttpUtility.UrlEncodeUnicode(content));
            msg.AppendFormat("{0}={1}&", "wysiwyg", "0");
			if (icon != 0)
			{
				msg.AppendFormat("{0}={1}&", "iconid", icon.ToString());
			}
			msg.AppendFormat("{0}={1}&", "s", "");
            msg.AppendFormat("{0}={1}&", "do", "postreply");
			msg.AppendFormat("{0}={1}&", "t", threadId.ToString());
			msg.AppendFormat("{0}={1}&", "p", "");
            msg.AppendFormat("{0}={1}&", "specifiedpost", "0");
            msg.AppendFormat("{0}={1}&", "parseurl", "1");
			msg.AppendFormat("{0}={1}&", "posthash", "invalid posthash");
			msg.AppendFormat("{0}={1}&", "poststarttime", "0");
			msg.AppendFormat("{0}={1}&", "multiquoteempty", "");
			msg.AppendFormat("{0}={1}&", "sbutton", "Submit Reply");
			msg.AppendFormat("{0}={1}&", "emailupdate", "0");
			msg.AppendFormat("{0}={1}&", "folderid", "0");
			msg.AppendFormat("{0}={1}&", "rating", "0");
			if (lockit)
			{
				msg.AppendFormat("{0}={1}&", "openclose", "1");
			}
			msg.AppendFormat("{0}={1}", "loggedinuser", GetOurUserId(cs));
			cs.Url = String.Format("{0}newreply.php?do=postreply&t={1}", ForumURL, threadId);
			cs.Data = msg.ToString();
            cs.Headers.Add("X-Requested-With", "XMLHttpRequest");
			//Trace.TraceInformation("Posting: " + cs.Data);
			String resp = HtmlHelper.PostToUrl(cs);
			if (resp == null)
			{
				// failure
				return false;
			}

			return true;
		}

        protected virtual String GetOurUserId(ConnectionSettings cs)
        {
            String rc = cs.CC.GetCookies(new System.Uri(ForumURL))["bbuserid"].Value;
            return rc;
        }


		internal void GetPostersLike(String name, Action<String, IEnumerable<Poster>> callback)
		{
			Event<String, Action<String, IEnumerable<Poster>>> evt = new Event<string, Action<String, IEnumerable<Poster>>>("GetPostersLike", name, callback);
			PostEvent(evt);
		}
		

		void DoGetPostersLike(string name, Action<String, IEnumerable<Poster> > callback)
		{
			/* headers
				POST /ajax.php?do=usersearch
				Origin: http://forumserver.twoplustwo.com
				Referer: http://forumserver.twoplustwo.com/private.php?do=newpm
				X-Requested-With: XMLHttpRequest

			request:
				securitytoken=1348471374-888f10c7477666008f130e5b77b9f6958b5c73f1&do=usersearch&fragment=aks
securitytoken	blah
do	usersearch
fragment	name

			 * */
			List<Poster> posters = new List<Poster>();
			ConnectionSettings cs = _connectionSettings.Clone();
			cs.Url = ForumURL + "private.php";
			String doc = HtmlHelper.GetUrlResponseString(cs);
			if (doc == null)
			{
				return;
			}
			// parse out securitytoken
			Regex reg = new Regex("var SECURITYTOKEN = \"(.+)\"");
			String securityToken = "";
			Match match = reg.Match(doc);
			if (match.Success)
			{
				securityToken = match.Groups[1].Value;
			}
			//			var ajax_last_post = 1338251071;
			String ajaxLastPost = "";
			reg = new Regex("var ajax_last_post = (.+);");
			match = reg.Match(doc);
			if (match.Success)
			{
				ajaxLastPost = match.Groups[1].Value;
			}
			/*
securitytoken	blah
do	usersearch
fragment	name
*/
			StringBuilder msg = new StringBuilder();
			msg.AppendFormat("{0}={1}&", "securitytoken", securityToken);
			msg.AppendFormat("{0}={1}&", "do", "usersearch");
			msg.AppendFormat("{0}={1}&", "fragment", name);
			cs.Url = String.Format("{0}/ajax.php?do=usersearch", ForumURL);
			cs.Data = msg.ToString();
			//Trace.TraceInformation("Posting: " + cs.Data);
			String resp = HtmlHelper.PostToUrl(cs);
			if (resp == null)
			{
				// failure
				return;
			}
			//Trace.TraceInformation(resp);
			/*
<?xml version="1.0" encoding="windows-1252"?>
<users>
	<user userid="124403">chi</user>
	<user userid="115734">chi psi 3</user>
	<user userid="144528">chi-town-playa</user>
	<user userid="335619">CHI23</user>
	<user userid="46484">chi23town</user>
	<user userid="227335">Chi2Tex</user>
	<user userid="324970">chi939</user>
	<user userid="12379">chiachu</user>
	<user userid="158696">chiagopoker666</user>
	<user userid="239621">Chiangsman</user>
	<user userid="339854">chiaog</user>
	<user userid="89930">chiapoker</user>
	<user userid="215389">chiapom28</user>
	<user userid="329156">chiaradiapoker</user>
	<user userid="33302">chiaroscuro</user>
</users>
*/
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(resp);
			XmlNodeList list = xml.SelectNodes("//user");
			if (list != null)
			{
				foreach (XmlNode node in list)
				{
					String sid = node.Attributes["userid"].Value;
					if (sid != null)
					{
						Int32 id = Int32.Parse(sid);
						String poster = node.InnerText;
						if (poster.Contains("amp"))
						{
							poster = HtmlAgilityPack.HtmlEntity.DeEntitize(poster);
						}
						Poster p = new Poster(poster, id);
						posters.Add(p);
					}
				}
			}
			callback(name, posters);
		}

        public virtual string ForumLobby
        {
            get;
            private set;
        }

        public string ForumHost { 
            get; 
            private set; 
        }

        public string ForumURL { 
            get
            {
                String rc = String.Format("http://{0}/", ForumHost);
                return rc;
            }
        }
    }
	public class VBulletinForum
	{
		#region members
		VBulletinSM _inner;
		Action<Action> _synchronousInvoker;
		#endregion
		#region constructors
		public VBulletinForum(Action<Action> synchronousInvoker, String forum, String vbVersion, String lobby)
		{
			_synchronousInvoker = synchronousInvoker;
            switch (vbVersion)
            {
                case "4.2.0":
                    {
                        _inner = new VBulletin_4_2_0(this, new StateMachineHost("ForumHost"), forum, lobby, synchronousInvoker);
                    }
                    break;

                default:
                    {
                        _inner = new VBulletin_3_8_7(this, new StateMachineHost("ForumHost"), forum, lobby, synchronousInvoker);
                    }
                    break;
            }
		}
		#endregion
		#region events
		public event EventHandler<LoginEventArgs> LoginEvent;
		virtual internal void OnLoginEvent(String username, LoginEventType let)
		{
			var handler = LoginEvent;
			if (handler != null)
			{
				LoginEventArgs e = new LoginEventArgs(username, let);
				_synchronousInvoker.Invoke(
					() => handler(this, e)
				);
			}

		}
		public event EventHandler<NewStatusEventArgs> StatusUpdate;
		virtual internal void OnStatusUpdate(String status)
		{
			var handler = StatusUpdate;
			if (handler != null)
			{
				_synchronousInvoker.Invoke(
					() => handler(this, new NewStatusEventArgs(status))
				);
			}
		}
		
		#endregion
		public Int32 PostsPerPage
		{
			get
			{
				return _inner.PostsPerPage;
			}
		}

		#region public methods
		public ThreadReader Reader()
		{
			ThreadReader t = _inner.Reader();
			return t;
		}
		public LobbyReader Lobby()
		{
			LobbyReader lr = _inner.Lobby();
			return lr;
		}
		public void Login(string user, string password)
		{
			_inner.Login(user, password);
		}
		public void Logout()
		{
			_inner.Logout();
		}
		public Boolean SendPM(IEnumerable<String> To, IEnumerable<String> bcc, String title, String content, Boolean receipt = true)
		{
            Boolean rc = _inner.SendPM(To, bcc, title, content, receipt);
            return rc;
		}
		public Boolean MakePost(Int32 threadId, String title, String message, Int32 PostIcon, Boolean LockThread)
		{
			Boolean rc = _inner.MakePost(threadId, title, message, LockThread, PostIcon);
			return rc;
		}
        public Boolean DeleteThread(Int32 postId)
        {
            Boolean rc = _inner.DeleteThread(postId);
            return rc;
        }
        public String NewThread(Int32 forum, String title, String body, Int32 icon, Boolean lockit)
        {
            String rc = _inner.NewThread(forum, title, body, icon, lockit);
            return rc;
        }
        public Boolean CanUserReceivePM(String name)
        {
            Boolean rc = _inner.CanUserReceivePM(name);
            return rc;
        }
		public void GetPostersLike(string name, Action<String, IEnumerable<Poster>> callback)
		{
			_inner.GetPostersLike(name, callback);
		}
		public static Int32 ThreadIdFromUrl(String url)
		{
			// Thread: -.../
			url = url.Trim();
			if (url.Length == 0)
			{
				return 0;
			}
			if (url.EndsWith(".html") || url.EndsWith(".htm"))
			{
				url = url.Substring(0, url.LastIndexOf("index"));
			}
			int ixTidStart = url.LastIndexOf('-') + 1;
			string tid = url.Substring(ixTidStart, url.Length - (ixTidStart + 1));
			Int32 threadId = 0;
			Int32.TryParse(tid, out threadId);
            if (0 == threadId)
            {
                // Estonia?
                int ixTidEnd = url.IndexOf('-');
                tid = url.Substring(0, ixTidEnd);
                ixTidStart = tid.LastIndexOf('/') + 1;
                tid = tid.Substring(ixTidStart, ixTidEnd - ixTidStart);
                Int32.TryParse(tid, out threadId);
            }
			return threadId;
		}
		#endregion




		public object LoginStatus { get; set; }
		public String ForumURL
		{
			get
			{
                String rc = _inner.ForumURL;
				return rc;
			}
		}
        public String ForumLobby
        {
            get
            {
                String rc = _inner.ForumLobby;
                return rc;
            }
        }
		public String ForumHost
		{
			get
            {
                String rc = _inner.ForumHost;
                return rc;
            }
		}
	}
	public class NewStatusEventArgs : EventArgs
	{
		public String Status
		{
			get;
			private set;
		}
		public NewStatusEventArgs(String status)
		{
			Status = status;
		}
	}
	public class PageCompleteEventArgs : EventArgs
	{
		public String URL
		{
			get;
			private set;
		}
		public Int32 Page
		{
			get;
			private set;
		}
		public Int32 TotalPages
		{
			get;
			private set;
		}
		public DateTimeOffset TimeStamp
		{
			get;
			private set;
		}
		public Posts Posts
		{
			get;
			private set;
		}
		public object Cookie
		{
			get;
			private set;
		}
		public PageCompleteEventArgs(String url, Int32 page, Int32 totalPages, DateTimeOffset ts, Posts posts, object o)
		{
			URL = url;
			Page = page;
			TotalPages = totalPages;
			TimeStamp = ts;
			Posts = posts;
			Cookie = o;
		}
	}
	public class NewPMEventArgs : EventArgs
	{
		public PrivateMessage PM
		{
			get;
			private set;
		}
		public NewPMEventArgs(PrivateMessage pm)
		{
			PM = pm;
		}
	}
	public class PMReadEventArgs : EventArgs
	{
		public PrivateMessage PM
		{
			get;
			private set;
		}
		public String Reader
		{
			get;
			private set;
		}
		public DateTime Time
		{
			get;
			private set;
		}
		public PMReadEventArgs(PrivateMessage pm, String reader, DateTime when)
		{
			PM = pm;
			Reader = reader;
			Time = when;
		}
	}
	public enum LoginEventType
	{
		LoginFailure = 0,
		LoginSuccess = 1,
		LogoutFailure = 2,
		LogoutSuccess = 3,
	}
	public class LoginEventArgs : EventArgs
	{
		public String Username
		{
			get;
			private set;
		}
		public LoginEventType LoginEventType
		{
			get;
			private set;
		}
		public LoginEventArgs(String username, LoginEventType let)
		{
			Username = username;
			LoginEventType = let;
		}
	}

}
