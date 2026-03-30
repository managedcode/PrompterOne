using System.Globalization;
using PrompterLive.Core.Localization;

namespace PrompterLive.Shared.Localization;

public static class UiTextCatalog
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<UiTextKey, string>> LocalizedValues =
        new Dictionary<string, IReadOnlyDictionary<UiTextKey, string>>(StringComparer.Ordinal)
        {
            [AppCultureCatalog.EnglishCultureName] = BuildEnglish(),
            [AppCultureCatalog.UkrainianCultureName] = BuildUkrainian(),
            [AppCultureCatalog.FrenchCultureName] = BuildFrench(),
            [AppCultureCatalog.SpanishCultureName] = BuildSpanish(),
            [AppCultureCatalog.PortugueseCultureName] = BuildPortuguese(),
            [AppCultureCatalog.ItalianCultureName] = BuildItalian()
        };

    public static string Get(UiTextKey key)
    {
        var cultureName = AppCultureCatalog.ResolveSupportedCulture(CultureInfo.CurrentUICulture.Name);
        var languageValues = LocalizedValues[cultureName];
        return languageValues.TryGetValue(key, out var localizedValue)
            ? localizedValue
            : LocalizedValues[AppCultureCatalog.DefaultCultureName][key];
    }

    private static IReadOnlyDictionary<UiTextKey, string> BuildEnglish() =>
        new Dictionary<UiTextKey, string>
        {
            [UiTextKey.DiagnosticsDismiss] = "Dismiss",
            [UiTextKey.DiagnosticsRetry] = "Retry",
            [UiTextKey.DiagnosticsLibrary] = "Library",
            [UiTextKey.DiagnosticsFatalTitle] = "Unexpected Error",
            [UiTextKey.DiagnosticsFatalMessage] = "PrompterLive hit an unexpected error. Try the screen again or return to the library.",
            [UiTextKey.DiagnosticsRecoverableTitle] = "Action Failed",
            [UiTextKey.LibraryAllScripts] = "All Scripts",
            [UiTextKey.LibraryRecent] = "Recent",
            [UiTextKey.LibraryFavorites] = "Favorites",
            [UiTextKey.LibraryFolders] = "Folders",
            [UiTextKey.LibraryNewFolder] = "New folder",
            [UiTextKey.LibrarySettings] = "Settings",
            [UiTextKey.LibrarySortBy] = "Sort by",
            [UiTextKey.LibrarySortName] = "Name",
            [UiTextKey.LibrarySortDate] = "Date",
            [UiTextKey.LibrarySortDuration] = "Duration",
            [UiTextKey.LibrarySortWpm] = "WPM",
            [UiTextKey.LibraryCreateFolderTitle] = "Create Folder",
            [UiTextKey.LibraryCreateFolderDescription] = "Organize scripts into nested collections without disturbing the library grid.",
            [UiTextKey.HeaderSearchPlaceholder] = "Search...",
            [UiTextKey.HeaderNewScript] = "New Script",
            [UiTextKey.HeaderLearn] = "Learn",
            [UiTextKey.HeaderRead] = "Read",
            [UiTextKey.HeaderGoLive] = "Go Live",
            [UiTextKey.HeaderSettings] = "Settings",
            [UiTextKey.GoLiveHeroEyebrow] = "LIVE ROUTING",
            [UiTextKey.GoLiveHeroDescription] = "Arm destinations for the current program feed while leaving teleprompter and RSVP ready in separate tabs.",
            [UiTextKey.CommonName] = "Name",
            [UiTextKey.CommonParent] = "Parent",
            [UiTextKey.CommonTopLevel] = "Top Level",
            [UiTextKey.CommonCancel] = "Cancel",
            [UiTextKey.CommonCreate] = "Create",
            [UiTextKey.LibraryFolderPlaceholder] = "Roadshows"
        };

    private static IReadOnlyDictionary<UiTextKey, string> BuildUkrainian() =>
        new Dictionary<UiTextKey, string>
        {
            [UiTextKey.DiagnosticsDismiss] = "Закрити",
            [UiTextKey.DiagnosticsRetry] = "Спробувати знову",
            [UiTextKey.DiagnosticsLibrary] = "Бібліотека",
            [UiTextKey.DiagnosticsFatalTitle] = "Неочікувана помилка",
            [UiTextKey.DiagnosticsFatalMessage] = "У PrompterLive сталася неочікувана помилка. Спробуйте екран ще раз або поверніться до бібліотеки.",
            [UiTextKey.DiagnosticsRecoverableTitle] = "Дію не виконано",
            [UiTextKey.LibraryAllScripts] = "Усі сценарії",
            [UiTextKey.LibraryRecent] = "Недавні",
            [UiTextKey.LibraryFavorites] = "Обране",
            [UiTextKey.LibraryFolders] = "Папки",
            [UiTextKey.LibraryNewFolder] = "Нова папка",
            [UiTextKey.LibrarySettings] = "Налаштування",
            [UiTextKey.LibrarySortBy] = "Сортувати за",
            [UiTextKey.LibrarySortName] = "Назвою",
            [UiTextKey.LibrarySortDate] = "Датою",
            [UiTextKey.LibrarySortDuration] = "Тривалістю",
            [UiTextKey.LibrarySortWpm] = "WPM",
            [UiTextKey.LibraryCreateFolderTitle] = "Створити папку",
            [UiTextKey.LibraryCreateFolderDescription] = "Організуйте сценарії у вкладені колекції, не порушуючи сітку бібліотеки.",
            [UiTextKey.HeaderSearchPlaceholder] = "Пошук...",
            [UiTextKey.HeaderNewScript] = "Новий сценарій",
            [UiTextKey.HeaderLearn] = "Вивчати",
            [UiTextKey.HeaderRead] = "Читати",
            [UiTextKey.HeaderGoLive] = "Прямий ефір",
            [UiTextKey.HeaderSettings] = "Налаштування",
            [UiTextKey.GoLiveHeroEyebrow] = "МАРШРУТИЗАЦІЯ ЕФІРУ",
            [UiTextKey.GoLiveHeroDescription] = "Налаштуйте виходи для поточного програмного сигналу, залишивши телесуфлер і RSVP готовими в окремих вкладках.",
            [UiTextKey.CommonName] = "Назва",
            [UiTextKey.CommonParent] = "Батьківська папка",
            [UiTextKey.CommonTopLevel] = "Верхній рівень",
            [UiTextKey.CommonCancel] = "Скасувати",
            [UiTextKey.CommonCreate] = "Створити",
            [UiTextKey.LibraryFolderPlaceholder] = "Роудшоу"
        };

    private static IReadOnlyDictionary<UiTextKey, string> BuildFrench() =>
        new Dictionary<UiTextKey, string>
        {
            [UiTextKey.DiagnosticsDismiss] = "Fermer",
            [UiTextKey.DiagnosticsRetry] = "Réessayer",
            [UiTextKey.DiagnosticsLibrary] = "Bibliothèque",
            [UiTextKey.DiagnosticsFatalTitle] = "Erreur inattendue",
            [UiTextKey.DiagnosticsFatalMessage] = "PrompterLive a rencontré une erreur inattendue. Réessayez cet écran ou revenez à la bibliothèque.",
            [UiTextKey.DiagnosticsRecoverableTitle] = "Action échouée",
            [UiTextKey.LibraryAllScripts] = "Tous les scripts",
            [UiTextKey.LibraryRecent] = "Récents",
            [UiTextKey.LibraryFavorites] = "Favoris",
            [UiTextKey.LibraryFolders] = "Dossiers",
            [UiTextKey.LibraryNewFolder] = "Nouveau dossier",
            [UiTextKey.LibrarySettings] = "Paramètres",
            [UiTextKey.LibrarySortBy] = "Trier par",
            [UiTextKey.LibrarySortName] = "Nom",
            [UiTextKey.LibrarySortDate] = "Date",
            [UiTextKey.LibrarySortDuration] = "Durée",
            [UiTextKey.LibrarySortWpm] = "MPM",
            [UiTextKey.LibraryCreateFolderTitle] = "Créer un dossier",
            [UiTextKey.LibraryCreateFolderDescription] = "Organisez les scripts dans des collections imbriquées sans perturber la grille de la bibliothèque.",
            [UiTextKey.HeaderSearchPlaceholder] = "Rechercher...",
            [UiTextKey.HeaderNewScript] = "Nouveau script",
            [UiTextKey.HeaderLearn] = "Apprendre",
            [UiTextKey.HeaderRead] = "Lire",
            [UiTextKey.HeaderGoLive] = "En direct",
            [UiTextKey.HeaderSettings] = "Paramètres",
            [UiTextKey.GoLiveHeroEyebrow] = "ROUTAGE EN DIRECT",
            [UiTextKey.GoLiveHeroDescription] = "Préparez les destinations pour le flux programme actuel tout en gardant le prompteur et RSVP prêts dans des onglets séparés.",
            [UiTextKey.CommonName] = "Nom",
            [UiTextKey.CommonParent] = "Parent",
            [UiTextKey.CommonTopLevel] = "Niveau supérieur",
            [UiTextKey.CommonCancel] = "Annuler",
            [UiTextKey.CommonCreate] = "Créer",
            [UiTextKey.LibraryFolderPlaceholder] = "Roadshows"
        };

    private static IReadOnlyDictionary<UiTextKey, string> BuildSpanish() =>
        new Dictionary<UiTextKey, string>
        {
            [UiTextKey.DiagnosticsDismiss] = "Cerrar",
            [UiTextKey.DiagnosticsRetry] = "Reintentar",
            [UiTextKey.DiagnosticsLibrary] = "Biblioteca",
            [UiTextKey.DiagnosticsFatalTitle] = "Error inesperado",
            [UiTextKey.DiagnosticsFatalMessage] = "PrompterLive encontró un error inesperado. Vuelve a intentar esta pantalla o regresa a la biblioteca.",
            [UiTextKey.DiagnosticsRecoverableTitle] = "Acción fallida",
            [UiTextKey.LibraryAllScripts] = "Todos los guiones",
            [UiTextKey.LibraryRecent] = "Recientes",
            [UiTextKey.LibraryFavorites] = "Favoritos",
            [UiTextKey.LibraryFolders] = "Carpetas",
            [UiTextKey.LibraryNewFolder] = "Nueva carpeta",
            [UiTextKey.LibrarySettings] = "Configuración",
            [UiTextKey.LibrarySortBy] = "Ordenar por",
            [UiTextKey.LibrarySortName] = "Nombre",
            [UiTextKey.LibrarySortDate] = "Fecha",
            [UiTextKey.LibrarySortDuration] = "Duración",
            [UiTextKey.LibrarySortWpm] = "PPM",
            [UiTextKey.LibraryCreateFolderTitle] = "Crear carpeta",
            [UiTextKey.LibraryCreateFolderDescription] = "Organiza los guiones en colecciones anidadas sin alterar la cuadrícula de la biblioteca.",
            [UiTextKey.HeaderSearchPlaceholder] = "Buscar...",
            [UiTextKey.HeaderNewScript] = "Nuevo guion",
            [UiTextKey.HeaderLearn] = "Estudiar",
            [UiTextKey.HeaderRead] = "Leer",
            [UiTextKey.HeaderGoLive] = "En vivo",
            [UiTextKey.HeaderSettings] = "Configuración",
            [UiTextKey.GoLiveHeroEyebrow] = "RUTEO EN VIVO",
            [UiTextKey.GoLiveHeroDescription] = "Prepara los destinos para la señal de programa actual mientras mantienes el teleprompter y RSVP listos en pestañas separadas.",
            [UiTextKey.CommonName] = "Nombre",
            [UiTextKey.CommonParent] = "Principal",
            [UiTextKey.CommonTopLevel] = "Nivel superior",
            [UiTextKey.CommonCancel] = "Cancelar",
            [UiTextKey.CommonCreate] = "Crear",
            [UiTextKey.LibraryFolderPlaceholder] = "Roadshows"
        };

    private static IReadOnlyDictionary<UiTextKey, string> BuildPortuguese() =>
        new Dictionary<UiTextKey, string>
        {
            [UiTextKey.DiagnosticsDismiss] = "Fechar",
            [UiTextKey.DiagnosticsRetry] = "Tentar novamente",
            [UiTextKey.DiagnosticsLibrary] = "Biblioteca",
            [UiTextKey.DiagnosticsFatalTitle] = "Erro inesperado",
            [UiTextKey.DiagnosticsFatalMessage] = "O PrompterLive encontrou um erro inesperado. Tente esta tela novamente ou volte para a biblioteca.",
            [UiTextKey.DiagnosticsRecoverableTitle] = "Ação falhou",
            [UiTextKey.LibraryAllScripts] = "Todos os roteiros",
            [UiTextKey.LibraryRecent] = "Recentes",
            [UiTextKey.LibraryFavorites] = "Favoritos",
            [UiTextKey.LibraryFolders] = "Pastas",
            [UiTextKey.LibraryNewFolder] = "Nova pasta",
            [UiTextKey.LibrarySettings] = "Configurações",
            [UiTextKey.LibrarySortBy] = "Ordenar por",
            [UiTextKey.LibrarySortName] = "Nome",
            [UiTextKey.LibrarySortDate] = "Data",
            [UiTextKey.LibrarySortDuration] = "Duração",
            [UiTextKey.LibrarySortWpm] = "PPM",
            [UiTextKey.LibraryCreateFolderTitle] = "Criar pasta",
            [UiTextKey.LibraryCreateFolderDescription] = "Organize roteiros em coleções aninhadas sem mexer na grade da biblioteca.",
            [UiTextKey.HeaderSearchPlaceholder] = "Pesquisar...",
            [UiTextKey.HeaderNewScript] = "Novo roteiro",
            [UiTextKey.HeaderLearn] = "Estudar",
            [UiTextKey.HeaderRead] = "Ler",
            [UiTextKey.HeaderGoLive] = "Ao vivo",
            [UiTextKey.HeaderSettings] = "Configurações",
            [UiTextKey.GoLiveHeroEyebrow] = "ROTEAMENTO AO VIVO",
            [UiTextKey.GoLiveHeroDescription] = "Prepare os destinos para o sinal de programa atual, mantendo o teleprompter e o RSVP prontos em abas separadas.",
            [UiTextKey.CommonName] = "Nome",
            [UiTextKey.CommonParent] = "Pai",
            [UiTextKey.CommonTopLevel] = "Nível superior",
            [UiTextKey.CommonCancel] = "Cancelar",
            [UiTextKey.CommonCreate] = "Criar",
            [UiTextKey.LibraryFolderPlaceholder] = "Roadshows"
        };

    private static IReadOnlyDictionary<UiTextKey, string> BuildItalian() =>
        new Dictionary<UiTextKey, string>
        {
            [UiTextKey.DiagnosticsDismiss] = "Chiudi",
            [UiTextKey.DiagnosticsRetry] = "Riprova",
            [UiTextKey.DiagnosticsLibrary] = "Libreria",
            [UiTextKey.DiagnosticsFatalTitle] = "Errore imprevisto",
            [UiTextKey.DiagnosticsFatalMessage] = "PrompterLive ha rilevato un errore imprevisto. Riprova questa schermata o torna alla libreria.",
            [UiTextKey.DiagnosticsRecoverableTitle] = "Azione non riuscita",
            [UiTextKey.LibraryAllScripts] = "Tutti gli script",
            [UiTextKey.LibraryRecent] = "Recenti",
            [UiTextKey.LibraryFavorites] = "Preferiti",
            [UiTextKey.LibraryFolders] = "Cartelle",
            [UiTextKey.LibraryNewFolder] = "Nuova cartella",
            [UiTextKey.LibrarySettings] = "Impostazioni",
            [UiTextKey.LibrarySortBy] = "Ordina per",
            [UiTextKey.LibrarySortName] = "Nome",
            [UiTextKey.LibrarySortDate] = "Data",
            [UiTextKey.LibrarySortDuration] = "Durata",
            [UiTextKey.LibrarySortWpm] = "PPM",
            [UiTextKey.LibraryCreateFolderTitle] = "Crea cartella",
            [UiTextKey.LibraryCreateFolderDescription] = "Organizza gli script in raccolte annidate senza alterare la griglia della libreria.",
            [UiTextKey.HeaderSearchPlaceholder] = "Cerca...",
            [UiTextKey.HeaderNewScript] = "Nuovo script",
            [UiTextKey.HeaderLearn] = "Studia",
            [UiTextKey.HeaderRead] = "Leggi",
            [UiTextKey.HeaderGoLive] = "In diretta",
            [UiTextKey.HeaderSettings] = "Impostazioni",
            [UiTextKey.GoLiveHeroEyebrow] = "INSTRADAMENTO IN DIRETTA",
            [UiTextKey.GoLiveHeroDescription] = "Prepara le destinazioni per il feed programma corrente, lasciando teleprompter e RSVP pronti in schede separate.",
            [UiTextKey.CommonName] = "Nome",
            [UiTextKey.CommonParent] = "Cartella padre",
            [UiTextKey.CommonTopLevel] = "Livello superiore",
            [UiTextKey.CommonCancel] = "Annulla",
            [UiTextKey.CommonCreate] = "Crea",
            [UiTextKey.LibraryFolderPlaceholder] = "Roadshows"
        };
}
