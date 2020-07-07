using Discord;
using Discord.Commands;
using GamersAddict.Models;
using RestSharp.Extensions.MonoHttp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GamersAddict
{
    internal class DiscordBot
    {
        // Variable
        DiscordClient discord;
        string m_lastLiveId = "";
        ///////////////////////

        public DiscordBot()
        {
            discord = new DiscordClient();
            discord.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
            });

            // -> Commands
            var commands = discord.GetService<CommandService>();

            // Aide
            commands.CreateCommand("Aide").Do(async (e) =>
            {
                await e.Channel.SendMessage("Bonjour @" + e.User + ", je suis Lopette, un Bot développé par Brouilles. Mon objectif et de vous aider au mieux dans vos démarches. Voilà les commandes que vous pouvez utiliser pour m'appeler :"
                    + "\n- !live : Je vous indique si un live est en cours sur la chaîne Youtube Gamers Addict."
                    + "\n- !article (optionnel: nombre d'article) : Je vous indique les derniers articles du site Gamers Addict. Vous pouvez en m'indiquant un numéro choisir le nombre à afficher."
                    + "\n- !cherche (obligatoire: insérer votre recherche) : Je trouve les articles en lien avec votre recherche.");
            });

            commands.CreateCommand("Lopette").Do(async (e) =>
            {
                await e.Channel.SendMessage("Bonjour @" + e.User + ", je suis Lopette, un Bot développé par Brouilles. Mon objectif et de vous aider au mieux dans vos démarches. Voilà les commandes que vous pouvez utiliser pour m'appeler :"
                    + "\n- !live : Je vous indique si un live est en cours sur la chaîne Youtube Gamers Addict."
                    + "\n- !article (optionnel: nombre d'article) : Je vous indique les derniers articles du site Gamers Addict. Vous pouvez en m'indiquant un numéro choisir le nombre à afficher."
                    + "\n- !cherche (obligatoire: insérer votre recherche) : Je trouve les articles en lien avec votre recherche.");
            });

            // Kara
            commands.CreateCommand("parle")
            .Do(async (e) =>
            {
                if (e.User.Name == "Brouilles")
                {
                    await e.Channel.SendMessage("```Joining!```");
                }
            });

            // Bonne nuit
            commands.CreateCommand("Bonne nuit").Do(async (e) =>
            {
                if (new Random().Next(0, 100) > 50)
                    await e.Channel.SendMessage($"Merci toi aussi @{e.User.Name} ! Même si je ne dors pas.");
                else
                    await e.Channel.SendMessage($"Fait de beau rêve @{e.User.Name}.");
            });

            // Amour
            commands.CreateCommand("Je t'aime").Do(async (e) =>
            {
                if (new Random().Next(0, 100) > 50)
                    await e.Channel.SendMessage("Moi aussi je t'aime <3 !");
                else
                    await e.Channel.SendMessage($"Hum, ça reste à voir {e.User.Name} !");
            });

            commands.CreateCommand("Je t'aime pas").Do(async (e) =>
            {
                if (new Random().Next(0, 100) > 50)
                    await e.Channel.SendMessage($"Pfff, tu ne me mérites pas de toute façon {e.User.Name} !");
                else
                    await e.Channel.SendMessage($"Va te faire, espèce de lopette du dimanche !");
            });

            commands.CreateCommand("Je ne t'aime pas").Do(async (e) =>
            {
                if (new Random().Next(0, 100) > 50)
                    await e.Channel.SendMessage($"Pfff, tu ne me mérites pas de toute façon {e.User.Name} !");
                else
                    await e.Channel.SendMessage($"Va te faire, espèce de lopette du dimanche !");
            });

            // Live
            commands.CreateCommand("Live").Do(async (e) =>
            {
                var modelConf = new Conf();
                using (var context = new SiteDbContext())
                {
                    modelConf = context.Conf.Find(1);
                }

                if (modelConf.Value == "")
                    await e.Channel.SendMessage("Gamers Addict n'est pas en live !");
                else
                    await e.Channel.SendMessage("Gamers Addict est en live sur Youtube, " + @modelConf.Name + " ! https://www.youtube.com/watch?v=" + modelConf.Value);
            });

            // Cherche
            commands.CreateCommand("Cherche")
                .Parameter("Msg", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string searchTerm = e.GetArg("Msg");

                    if (searchTerm != string.Empty || searchTerm.Length > 0)
                    {
                        await e.Channel.SendMessage("Je cherche " + searchTerm + "...");
                        // Get Data
                        string msg = "";
                        List<ArticlesViewModel> model;

                        using (var context = new SiteDbContext())
                        {
                            model = context.Articles
                                .OrderByDescending(r => r.Id)
                                .Where(r => r.Title.Trim().Contains(searchTerm) && r.PublishState == 2
                                    || r.Tags.Trim().Contains(searchTerm) && r.PublishState == 2)
                                .Select(r => new ArticlesViewModel
                                {
                                    Id = r.Id,
                                    Title = r.Title,
                                    Description = r.Description,
                                    Date = r.Date,
                                    Views = r.Views,
                                    PublishState = r.PublishState
                                }).Take(6).ToList();
                        }

                        msg += "Résultat de votre recherche (" + model.Count + " correspondance(s)) :";
                        foreach (var item in model)
                        {
                            msg += $"\n-**{ item.Title }** - { item.Description } - http://gamersaddict.fr/Article/detail/{ HttpUtility.UrlEncode(item.Title) }";
                        }
                        await e.Channel.SendMessage(msg);
                    }
                    else
                        await e.Channel.SendMessage("Il manque votre recherche après la commande ! Lopette va !");
                });

            // Dernier article
            commands.CreateCommand("Article")
                .Parameter("Number", ParameterType.Optional)
                .Do(async (e) =>
                {
                    int number = 3;
                    if (e.GetArg("Number") != String.Empty || e.GetArg("Number") != null)
                    {
                        bool success = Int32.TryParse(e.GetArg("Number"), out number);
                        if (success == false)
                            number = 3;

                        if (number > 10)
                            number = 10;
                    }

                    // Get Data
                    List<ArticlesViewModel> articleList;
                    using (var context = new SiteDbContext())
                    {
                        articleList = context.Articles
                            .OrderByDescending(r => r.Date)
                            .Where(r => r.PublishState == 2)
                            .Take(number)
                            .Select(r => new ArticlesViewModel
                            {
                                Id = r.Id,
                                Title = r.Title,
                                Description = r.Description,
                                Date = r.Date,
                                Views = r.Views,
                                PublishState = r.PublishState
                            }).ToList();
                    }

                    string msg = "Les articles récents sont les suivants :";
                    foreach (var item in articleList)
                    {
                        msg += $"\n-**{ item.Title }** - { item.Description } - http://gamersaddict.fr/Article/detail/{ HttpUtility.UrlEncode(item.Title) }";
                    }
                    await e.Channel.SendMessage(msg);
                });

            Task.Run(async () =>
            {
                await discord.Connect("MjgxMzE1MjY3MzE2MDIzMjk2.C4WLzw.Aucv6eUJ_eWwOHCLgIgTwRjuxOI", TokenType.Bot);
                discord.SetGame("Tape !aide pour l'aide");

                await Task.Delay(TimeSpan.FromSeconds(4));
                new System.Threading.Timer(async delegate
                {
                    var channel = discord.GetChannel(94889398809657344); //e.Server.FindChannels("general").FirstOrDefault();
                    if (channel != null)
                    {
                        var modelConf = new Conf();
                        using (var context = new SiteDbContext()) { modelConf = context.Conf.Find(1); }

                        if (modelConf.Value != "")
                        {
                            if (modelConf.Value != m_lastLiveId)
                            {
                                await channel.SendMessage("Gamers Addict est en live sur Youtube, " + @modelConf.Name + " ! https://www.youtube.com/watch?v=" + modelConf.Value);
                                m_lastLiveId = modelConf.Value;
                            }
                        }
                    }
                }, null, 0, (int)TimeSpan.FromMinutes(10.0).TotalMilliseconds);
            });
        }
    }
}