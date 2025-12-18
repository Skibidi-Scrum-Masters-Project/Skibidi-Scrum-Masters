Markdown

# Velkommen til FitLife Fitness PoC

Følg denne guide for at opsætte projektet lokalt eller tilgå vores cloud-løsning.

---

## 🛠 Lokal Opsætning

For at køre systemet lokalt skal du udføre følgende trin i din terminal:

### 1. Database og infrastruktur
Gå til static-mappen og start containerne:
```bash
cd /static
docker compose up -d
```
2. Testmiljø

Naviger til test-miljøet og byg applikationen:
Bash
````bash
cd /test_env
docker compose up --build -d
````
👤 Testbrugere (Seeded)

Følgende konti er præ-konfigureret i systemet:

Brugernavn: coach
Password: skibidicoach

Brugernavn: admin
Password: skibidiadmin
💾 Database Konfiguration

Hvis du ønsker at benytte den lokale database, skal Program.cs i hvert projekt opdateres med følgende MongoDB connection string:

mongodb://admin:abfmfitlifeskibidi@mongodb:27017/FitnessAppDB?authSource=admin
🌐 Cloud Adgang

Systemet kan tilgås direkte i skyen her: 👉 http://fitlife.qzz.io
⚠️ Proof of Concept (PoC) Begrænsninger

Da dette er en tidlig prototype, er følgende funktioner endnu ikke implementeret:

    Heatmap: Oversigt over hvornår centret er mest travlt.

    Wearables: Integration med fitness-ure og trackere.

    Lokationer: Understøttelse af flere fitnesscentre.

    Sikkerhed: RefreshToken funktionalitet. - Dette gør at man er nødsaget til at logge ud hver 90 minut og så logge ind igen for at få en ny jwt token

    Økonomi: Betalingsservice integration.
