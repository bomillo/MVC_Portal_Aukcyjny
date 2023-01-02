# PortalAukcyjny

Projekt realizowany jako zaliczenie z przedmiotu ASP MVC. Jest to aplikacja działająca jako portal aukcyjny (na wzór Allegro/olx). Umożliwia wystawianie produktów na auckję, branie udziału w aukcji oraz obserwowanie aukcji.

## Wykorzystane technologie
 - ### .NET 6
 - ### Entity Framework Core 6.0
 - ### ElasticSearch 8.0 
 - ### PostgreSQL 14.5
 - ### jQuery 3.5.1
 - ### wkhtmltopdf 
 - ### Sass (.scss)

## Osoby realizujące projekt
#### Adam Perkowski
 - Tworzenie aukcji
 - Synchronizacja kursów wymiany walut z serwisu NBP
 - Stronicowanie list auckji
 - Dodawanie nowych aukcji (backend)
 - Przetwarzanie obrazów pozyskanych od użytkowników (zmniejszanie i kompresja)
 - Mechanizm wyświetlania histori auckji użytkownika
 - Lista najaktywniejszych aukcji
 - Mechanizm oberwacji aukcji
 - Widok listy aukcji
 - Widok strony głównej 
 - Mechanizm edycji profilu 

#### Kamil Piętka
 - Dodawanie nowych aukcji (frontend)
 - Implementacja endpoint'ów API
 - Widok szczegółów aukcji
 - Tworzenie ofert do aukcji
 - Mechanizm zadawania pytań do aukcji
 - Generowanie raportów CSV/PDF
 - Wybór motywu
 - Nawigacja okruszkowa (breadcrumbs)
 - Cookies consent 
 - Google reChaptha
 - Mechanizm autentykacji komunikacji w API przy pomocy kluczy
 - Licznik odwiedzin portalu 
 - Przygotowanie struktury projektu (frontedn)
 - Integrajca zewnętrznych autentykatorów (Facebook, Google)
 
#### Dominik Postołowicz
 - Integracja ElasticSearch
 - Logowanie się użytkownika
 - Rejestracja 
 - Bezpieczna procedura przypominania hasła
 - Mechanizm wysyłki maili
 - Formularze kontatkowe
 - Przygotowanie projektu do globalizacji (tłumaczenia)
 - Wewnętrzny komunikator
 - Zarządanie środowiskiem produkcyjnym
 - Mechanizm obsługujący aukcje w czasie rzeczywistym (zmiany stanów, powiadamianie zainteresowanych o zmianach w obserwowanych aukcjach itd.)
