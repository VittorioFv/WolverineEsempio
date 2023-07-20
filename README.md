# WolverineEsempio
Per eseguire il progetto bisogna far partire SQL Server su Docker con:
```sh
docker compose up -d
```
E occorre far partire una prima migrazione:
Su Visual Studio:
```sh
Add-Migration Init
```
```sh
Update-Database
```
Nel *Package Manager Console*.

Di seguito una analisi con un confronto con *MediatR*.


# Analisi della libreria Wolverine
## Confronto con MediatR

Nonostante Wolverine non sia nata con lo scopo di essere utilizzato come mediator può essere usata anche con questo scopo.

Di seguito alcune delle differenze con MediatR, ci tengo a precisare però che Wolverine ha molte più funzionalita e [cerca di coprire molti più casi d'uso](https://jeremydmiller.com/2023/06/19/wolverines-middleware-strategy-is-a-different-animal/), soprattutto come *Message Bus*.

Questa è forse la differenza principale da tenere in considerazione perchè essendo "più grande" aggiunge inevitabilmente anche più complessità; complessità che per progetti piccoli o che non necessitano delle funzionalità aggiuntive di Wolverine potrebbe essere eccessiva.

| Wolverine | MediatR |
|-----------|---------|
|Licenza: [MIT](https://wolverine.netlify.app/) | Licenza: [Apache 2.0](https://github.com/jbogard/MediatR/blob/master/LICENSE)
| Non era inizialmente progettato per essere usato come [*mediator*](https://wolverine.netlify.app/guide/http/mediator.html#using-as-mediator). | Progettato praticamente per essere utilizzato come *mediator*. |
Wolverine usa la *naming convention* per trovare e assegnare gli [*handler*](https://wolverine.netlify.app/guide/handlers/discovery.html#handler-type-discovery). | MediatR necessita che vengano implementate delle interfaccie.
Supporta e incoraggia la [*method injection*](https://wolverine.netlify.app/tutorials/best-practices.html#best-practices) con classi e metodi di tipo *static*, ma supporta anche la *constructor injection*. | Solo *constuctor injection*.
| Gli handler di wolverine possono essere sia [asincroni che sincroni](https://wolverine.netlify.app/guide/handlers/return-values.html#return-values). | supporta solo metodi [asincroni](https://github.com/jbogard/MediatR/issues/267).
| Potresti avere [più handler](https://wolverine.netlify.app/guide/handlers/#message-handlers) per la stessa richiesta, verranno chiamati in sequenza in base a come vengono trovati. | Non puoi avere [più *IRequestHandler*](https://stackoverflow.com/questions/65151415/mediatr-multiple-requesthandlers) per una richiesta, ma si potrebbero usare [*INotificationHandler*](https://github.com/jbogard/MediatR/issues/163). 
| Non molto documentato e molta della documentazione ufficiale deve ancora essere prodotta ([1](https://wolverine.netlify.app/guide/durability/marten/operations.html#marten-operation-side-effects), [2](https://wolverine.netlify.app/guide/messaging/listeners.html#message-listeners), [3](https://wolverine.netlify.app/guide/messaging/transports/azureservicebus/deadletterqueues.html), [4](https://wolverine.netlify.app/guide/messaging/transports/sqs/queues.html), [5](https://wolverine.netlify.app/guide/messaging/broadcast-to-topic.html#scheduling-message-delivery)). In conpenso all'interno del progetto github ci sono diversi [esempi](https://wolverine.netlify.app/guide/samples.html#sample-projects) (alcuni non ancora spiegati o utilizzati nella documentazione). | Community molto ampia e molte risorse online non solo quelle ufficiali. 
| OGNI classe serializzabile (ad eccezione dei tipi primitivi) che un *handler* ritorna viene gestito come *Cascading Messages* e quindi vengono inviati ad altri handler o in qualche message queue o bus in base alla configurazione. | il valore viene ritornato normalmente nel caso di *IRequestHandler* mentre per *INotificationHandler* [non dovrebbe restistuire un valore](https://github.com/jbogard/MediatR/issues/163).
| [Local message bus](https://wolverine.netlify.app/guide/messaging/transports/local.html) | 
| [asynchronous messaging framework](https://wolverine.netlify.app/guide/messaging/introduction.html) | 
| Con la libreria [WolverineFx.Http](https://wolverine.netlify.app/guide/http/#http-services-with-wolverine), Wolverine potrebbe essere utilizzata come alternativa di *ASP.Net Core Endpoint provider* (Non analizzerò questo caso).

## Confronto command sul codice tra MediatR e Wolverine
In questo esempio si analizzerà principalmente L'implementazione con *Entity Framework* e *SQL Server* in un API *ASP.NET Core*.
Wolverine supporta anche *PostgreSQL* con [*Marten*](https://martendb.io/).
Wolverine [utilizza il *.Net Generic Host*](https://wolverine.netlify.app/tutorials/getting-started.html#getting-started) quindi è relativamente indipendente da *framework* come *ASP.NET Core*.

### MediatR
#### Configurazione
```cs
builder.Services.AddMediatR(x => 
    x.RegisterServicesFromAssemblyContaining<Program>());

builder.Services.AddDbContext<ItemDbContext>(x =>
{
    x.UseSqlServer(connectionString);
});
```
#### Controller:
```cs

[ApiController]
[Route("[controller]")]
public class ItemController : ControllerBase
{
    private IMediator _mediator;

    public ItemController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateItem(Item item)
    {
        if (item.Name == "")
        {
            return BadRequest();
        }

        var command = new CreateItemCommand(item);

        await _mediator.Send(command);

        return Ok();
    }
}
```

#### Command
```cs
public record CreateItemCommand(Item Item) : IRequest;
```
#### Handler
```cs
public class CreateItemHandler : IRequestHandler<CreateItemCommand>
{
    ItemDbContext _dbContext;

    public CreateItemHandler(ItemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(CreateItemCommand command, CancellationToken cancellationToken)
    {
        Item item = command.Item;
        item.Id = Guid.NewGuid();

        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync();

        await Console.Out.WriteLineAsync($@"

    ITEM CREATO: {item.Id}

");

        await Console.Out.WriteLineAsync($@"

    Faccio qualcosa dopo che:
        {item.Id}
    è stato creato.

");
    }
}
```

### Wolverine
#### Configurazione
```cs
builder.Host.UseWolverine(opt =>
{
    opt.PublishMessage<ItemCreated>().ToLocalQueue("important");
    opt.Policies.AllLocalQueues(x => x.UseDurableInbox());

    opt.UseEntityFrameworkCoreTransactions();
    opt.PersistMessagesWithSqlServer(connectionString);
});

builder.Services.AddDbContextWithWolverineIntegration<ItemDbContext>(x =>
{
    x.UseSqlServer(connectionString);
});

builder.Host.UseResourceSetupOnStartup();

builder.Host.ApplyOaktonExtensions();
```
In poche parole sto dicendo al programma di salvare sul database tutti i messaggi di tipo `ItemCreated` nella *Queue* locale come "important".
Di utilizzare la *DurableInbox*, che non è altro che una strategia di persistenza (più dettagli verranno spiegati più avanti), in tutte le *Queue* e di farlo utilizzando *Entity Framework* e *SQL Server*.

Nota: Wolverine quando viene chiamato `UseWolverine()` va a sovrascrivere la *dependency injection* precedente con la *dependency injection* di Lamar. Questo non si nota neanche e si potrebbe anche non sapere; l'unica differenza che ho notato è che alcune volte alcune cose che sarebbero normalmente *Scoped* diventano *Transient* o *Singleton*; in ogni caso se si rimane a utilizzare `AddDbContextWithWolverineIntegration` e le impostazioni standard che Woleverine suggerisce in fase di configurazione queste cose vengono settate di default.

Wolverine utilizza anche [Oakton](https://wolverine.netlify.app/guide/durability/managing.html#managing-message-storage) Quindi potrebbe essere necessario inserire:
```cs
builder.Host.UseResourceSetupOnStartup();

builder.Host.ApplyOaktonExtensions();
```
e aggiungendo:
```cs
await app.RunOaktonCommands(args);
```
al posto di:
```cs
await app.RunAsync();
```
si hanno delle funzionalità in più, non è obbligatorio ma rende la convivenza con *Wolverine* un po' più accettabile.

#### Controller
```cs
[ApiController]
[Route("[controller]")]
public class ItemController : ControllerBase
{
    private IMessageBus _bus;

    public ItemController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpPost]
    public async Task<IActionResult> CreateItem(Item item)
    {
        if (item.Name == "")
        {
            return BadRequest();
        }

        var command = new CreateItemCommand(item);

        await _bus.InvokeAsync(command);

        return Ok();
    }
}
```
Wolverine si occuperà della *dependency injection* di ```IMessageBus``` nel metodo ```UseWolverine()```.
Il metodo ```IMessageBus.InvokeAsync(command)``` (da non confondere con ```IMessageBus.SendAsync(command)``` o ```IMessageBus.PublishAsync(command)```) fa gestire il messaggio ```CreateItemCommand``` *inline* e nel thread corrente con Wolverine che agisce come [mediatore](https://wolverine.netlify.app/guide/runtime.html#invoking-a-message-inline).

#### Command
```cs
public record CreateItemCommand(Item Item);
```
#### Handler
```cs
[Transactional]
public static class CreateItemHandler
{
    public static async Task<ItemCreated> Handle(CreateItemCommand command, ItemDbContext dbContext)
    {
        Item item = command.Item;
        item.Id = Guid.NewGuid();

        dbContext.Items.Add(item);

        await Console.Out.WriteLineAsync($@"

    ITEM CREATO: {item.Id}

");

        return new ItemCreated((Guid)item.Id);//.ScheduledAt(DateTimeOffset);
    }
}
```
Da notare come non viene mai chiamato ```dbContext.SaveChangesAsync();```, questo perchè Wolverine si occupa già di questo.
In particolare utilizza i *transactional middleware* (da li l'attributo ```[Transactional]```). si potrebbe evitare di scrivere l'attributo inserendo in configurazione ```opts.Policies.AutoApplyTransactions();``` (vedi [esempio](https://wolverine.netlify.app/guide/durability/efcore.html#auto-apply-transactional-middleware)).
Questo serve a Wolverine per poter implementare correttamente l'[*outbox message persistence*](https://wolverine.netlify.app/guide/durability/efcore.html#entity-framework-core-integration).
In realtà non è necessario ma potrebbero esserci problemi con l'*outbox pattern* come scritto [qui](https://wolverine.netlify.app/guide/durability/marten/#transactional-middleware).

Da notare inoltre che nessuna interfaccia è stata implementata ma si basa tutto sulla *naming convention*; In particolare la classe *handler* deve finire con *Handler* o *Consumer* e il metodo deve chiamarsi *Handle* o *Consume*.

Rispetto al primo handler manca una parte di codice:
```cs
    await Console.Out.WriteLineAsync($@"

        Faccio qualcosa dopo che:
            {item.Id}
        è stato creato.

    ");
}
```
Invece c'è:
```cs
        return new ItemCreated((Guid)item.Id);
    }
}
```

Questo perchè si usa il sistema di *Message Bus* integrato in Wolverine.
Il ```return``` in un handler di Wolverine non fa altro che inviare il messaggio antistante (in questo caso un ```ItemCreated```) al sistema di messaggistica impostato nella configurazione.
Questa azione viene chiamata da Wolverine: [*Cascading Messages*](https://wolverine.netlify.app/guide/handlers/cascading.html#cascading-messages).

In questo caso ```ItemCreated``` viene inviato alla *Queue* locale utilizzando la *durable inbox* e quindi viene salvato sul database (se in configurazione non si predispone *durable outbox/inbox* il tutto sarà salvato in memoria e verrà perso per sempre se il programma si spegne o si blocca).

Ok, ma dopo cosa se ne fa di questo messaggio?
In questo caso trattandosi di una *Queue* locale, sarà lo stesso programma a prendersi in carico la gestione del messaggio (Non sono sicuro che debba essere lo stesso programma; però non ho trovato nulla che permetta di utilizzare una Queue locale esternamente).
Mentre se si pubblicassero i messaggi su *RabbitMQ*, *AWS SQS*, *Azure Service Bus* o utilizzando il sistema *built in* che utilizza *TCP Transport* potrebbero essere altri programmi a prendersi in carico la gestione del messaggio e Wolverine si occuperebbe solo di inviarli.

In questo caso all'interno del progetto esiste una classe chiamata ```ItemCreatedConsumer```:
```cs
public static class ItemCreatedConsumer
{
    public static async Task Consume(ItemCreated itemCreated)
    {
        await Console.Out.WriteLineAsync($@"

    Faccio qualcosa dopo che:
        {itemCreated.Id}
    è stato creato.

");
    }
}
```
Nota: potresti sostituire "Consumer" con "Handler" e "Consume" con "Handle" e non farebbe nessuna differenza.

Wolverine consapevole dell'esistenza di questo consumer/handler provvederà a eseguire il messaggio secondo le istruzioni che sono scritte all'interno dell'handler.

Visto che le *Local Queue* [si basano tutte sulla *TPL Dataflow library*](https://wolverine.netlify.app/guide/messaging/transports/local.html#publishing-messages-locally) questo processo verra gestito in maniera indipendente rispetto a chi invia (in questo caso ```CreateItemHandler```) Analizzerò più nel dettaglio questo argomento più avanti.

## Differenze sulle Query
Uguale a sopra ma per una Query dove ci si aspetta che venga ritornato un valore.

Le configurazioni sono identiche.

### MediatR
#### Controller
```cs
[ApiController]
[Route("[controller]")]
public class ItemController : ControllerBase
{
    private IMediator _mediator;

    public ItemController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetItems()
    {
        return Ok(await _mediator.Send(new GetItemsQuery()));
    }
}
```

#### Query
```cs
public record class GetItemsQuery() : IRequest<Item[]>;
```
#### Handler
```cs
public class GetItemsHandler : IRequestHandler<GetItemsQuery, Item[]>
{
    ItemDbContext _dbContext;

    public GetItemsHandler(ItemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Item[]> Handle(GetItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _dbContext.Items.OrderBy(x => x.Name).Take(10).ToArrayAsync();

        return items;
    }
}
```

Ritorna i primi 10 *Item* ordinati secondo il nome.

### Wolverine
#### Controller
```cs
[ApiController]
[Route("[controller]")]
public class ItemController : ControllerBase
{
    private IMessageBus _bus;

    public ItemController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpGet]
    public async Task<IActionResult> GetItems()
    {
        return Ok(await _bus.InvokeAsync<Item[]>(new GetItemsQuery()));
    }
}
```
Da notare che rispetto a prima il metodo `InvokeAsync()` è leggermente diverso:
`InvokeAsync()` nell'esempio precedente:
```cs
_bus.InvokeAsync(command);
```
in questo esempio:
```cs
_bus.InvokeAsync<Item[]>(new GetItemsQuery());
```
Ciò che sta dentro a ```<T>``` è il valore che verrà restutuito, se non è specificato nulla, come nel primo caso, non verrà restituito [nulla](https://wolverine.netlify.app/guide/messaging/message-bus.html#request-reply).

#### Query
```cs
public record class GetItemsQuery();
```
#### Handler
```cs
public static class GetItemsHandler
{
    public static async Task<Item[]> Handle(GetItemsQuery query, ItemDbContext dbContext)
    {
        Item[] items = await dbContext.Items.OrderBy(x => x.Name).Take(10).ToArrayAsync();

        // This code is only an example
        // Return items trigger the cascading messages
        return items;
    }
}
```
Questo codice potrebbe sembrare ok a una prima occhiata ma ha "un problema" (di per se non viene considerato un problema ma potrebbe crearne in casi particolari).

Il valore ritornato `Items` viene preso in considerazione come messaggio da Wolverine e Wolverine cercherà di gestirlo come tale.

La coferma di questo comportamento ce l'abbiamo dalla console che, ogni volta che viene chiamata la richiesta *get items* produce questo output:
```shell
info: Wolverine.Runtime.WolverineRuntime[107]
      No routes can be determined for Envelope #01892acc-3022-40f1-9696-fbd5647c3970 (Item)
info: Wolverine.Runtime.WolverineRuntime[107]
      No routes can be determined for Envelope #01892acc-3027-47f8-9b4b-426e0e894696 (Item)
info: Wolverine.Runtime.WolverineRuntime[107]
      No routes can be determined for Envelope #01892acc-3029-4638-9491-f246c1d760f7 (Item)
info: Wolverine.Runtime.WolverineRuntime[107]
      No routes can be determined for Envelope #01892acc-302a-4a90-bee9-fe865c839af4 (Item)
info: Wolverine.Runtime.WolverineRuntime[107]
      No routes can be determined for Envelope #01892acc-302c-4fcf-b687-83c066e37225 (Item)
info: Wolverine.Runtime.WolverineRuntime[107]
      No routes can be determined for Envelope #01892acc-302e-4643-b844-a1395ffd6dd0 (Item)
info: Wolverine.Runtime.WolverineRuntime[107]
      No routes can be determined for Envelope #01892acc-302f-4403-8c0d-807f0644c60b (Item)
info: Wolverine.Runtime.WolverineRuntime[107]
      No routes can be determined for Envelope #01892acc-3031-48ee-bd16-ba5bdd3916a7 (Item)
info: Wolverine.Runtime.WolverineRuntime[107]
      No routes can be determined for Envelope #01892acc-3033-4fa9-be29-519b22144b33 (Item)
info: Wolverine.Runtime.WolverineRuntime[107]
      No routes can be determined for Envelope #01892acc-3035-47be-8191-827b7fc001bb (Item)
```

Sta tentando di passare `Item` a qualche *Handler* Però non esiste nessun handler che gestisce `Item`.

Se vi state chiedendo perche ci sono 10 risposte la risposta è che visto che si ritorna un array ogni singolo *Item* della lista viene considerato come singolo messaggio; [per saperne di più.](https://wolverine.netlify.app/guide/handlers/return-values.html#return-values)

Ancora peggio se si considera il fatto che avendo impostato tutte le *Queue* locali come *Durable* questa chiamata viene salvata sul database. Questo si potrebbe evitare nella configurazione impostando solo alcune Queue come durable.

**Non ho trovato sulla documentazione qualcosa che permetta di ritornare il valore senza che venga passato anche come messaggio ammeno che non si usino delle *stuct* che non vengono prese in considerazione come messaggi.**

Un modo per evitare il problema è ritornare una classe non serializzabile.

Un'altra soluzione accettabile (almeno per le performace) sarebbe fare una chiamata direttamente dal dbcontext o utilizzare qualche altra classe/interfaccia che non sia un handler di Wolverine.

Per esempio modificando il controller in questo modo:
```cs
[HttpGet]
public async Task<IActionResult> GetItems()
{
    return Ok(await _dbContext.Items.OrderBy(x => x.Name).Take(10).ToArrayAsync());
}
```

Ovviamente in un contesto di *clean architecture* questo è oggettivamente una merda: Si sta mescolando il *presentation layer* e l'*application layer* e non si sta rispettando la dipendenza con il *layer* contenente il database (che dovrebbe essere invertita). (Ci tengo a precisare che il codice presente in questo github ha solo lo scopo di mostrare le differenze tra MediatR e Wolverine e analizzare quest'ultima libreria, non sta seguendo nessuna architettura specifica).
Di per se qualche interfaccia in più (per la *Dependency inversion*) e con qualche "rimescolamento" di codice si potrebbe rientrare nei principi della *clean architecture*.

Invece questo codice nello specifico potrebbe essere ok (non necessariamente ideale) se si usa un modello CQRS dove questa rischiesta specifica viene gestita nella maniera più efficente (senza tante mappature o passaggi di oggetti ad altri per nulla).

Ma perchè ho perso tempo a spiegare questa cosa: perchè è una delle maniere in cui il creatore di Wolverine (che inizialmente si chiamava Jasper) vorrebbe che Wolverine e Marten funzionassero insieme (creando così la *Critter Stack*).
Vi metto qui 2 articoli: 
[A Vision for Low Ceremony CQRS with Event Sourcing.](https://jeremydmiller.com/2022/06/15/a-vision-for-low-ceremony-cqrs-with-event-sourcing/)
[Critter Stack Roadmap (Marten, Wolverine, Weasel)](https://jeremydmiller.com/2023/03/02/critter-stack-roadmap-marten-wolverine-weasel/)
Dove scrive:
The goal for this year is to make the Critter Stack the best technical choice for a CQRS with Event Sourcing style architecture across every technical ecosystem.

Ovviamente sto semplificando molto ma il concetto di base è: se l'autore di Wolverine è così tanto interessato al modello CQRS sarà inevitabile che Wolverine evolverà anche in questa direzione (e potrebbe spiegare perchè gli esempi che ho analizzato dove viene usato Wolverine quasi nessuno lo usava per gestire le *Query*).

## Transactional outbox Pattern
Articolo di riferimento: [Transactional Outbox/Inbox with Wolverine and why you care](https://jeremydmiller.com/2022/12/15/transactional-outbox-inbox-with-wolverine-and-why-you-care/)
Pattern: [Pattern: Transactional outbox](https://microservices.io/patterns/data/transactional-outbox.html)

Non voglio spiegare il pattern o altro ma spiegare il motivo per cui Wolverine lo implementa e perchè si dovrebbe marcare come `[Transactional]` l'Handler.

Uno dei motivi principali per cui si usa questo pattern è che non ci si vuole trovare nella situazione dove il messaggio è stato inviato ma l'aggiornamento sul database non è avvenuto e viceversa.

Per questo motivo Wolverine gestisce l'update sul database e il salvataggio del messaggio in un unica transazione in modo che se fallisce fallisce in blocco, ed è anche il motivo per cui Wolverine richiede di essere associato al dbContext di *Entity Framework Core* in fase di Configurazione. (si potrebbe associare il *dbContext* anche [in maniera diversa](https://wolverine.netlify.app/guide/durability/efcore.html#outbox-outside-of-wolverine-handlers))

Ho semplificato molto, in ogni caso ci sono i link sopra per più informazioni.

## Error Handling
E' ovviamente inevitabile che non ci siano mai errori per questo Wolverine mette a disposizione una [serie di strumenti per affrontarli](https://wolverine.netlify.app/guide/handlers/error-handling.html#error-handling).

## Runtime Architecture
[Solo il link alla documentazione per comprendere meglio come funziona Wolverine. E perché Wolverine è "un animale differente" rispetto a MediatR](https://wolverine.netlify.app/guide/runtime.html#runtime-architecture)

## Messaging
Di seguito un esempio dove invio un messaggio su una porta TCP e lo ricevo con un altro *"Worker"* di un altro progetto. Di per se le logiche sono sempre le stesse (più dettagli [qui](https://wolverine.netlify.app/guide/messaging/transports/rabbitmq/deadletterqueues.html))

Dalla configurazione si definisce dove inviare i vari messaggi:
```cs
builder.Host.UseWolverine(opt =>
{
    opt.PublishMessage<ItemCreated>()
    .ToPort(5580)
    .UseDurableOutbox();
    
    opt.Policies.AllLocalQueues(x => x.UseDurableInbox());

    opt.UseEntityFrameworkCoreTransactions();
    opt.PersistMessagesWithSqlServer(connectionString);
});

```

Per inviarlo in altri posti l'idea è sempre di aggiungere `.To*DoveInviarlo*()` dopo `PublishMessage()` o `PublishAllMessages()`.

Basta.

Per il *Consumer* invece basta aggiungere un worker che sta in ascolto alla porta giusta:

```cs
var builder = Host.CreateDefaultBuilder(args)
    .UseWolverine(opts =>
    {
        // listen to incoming messages at port 5580
        opts.ListenAtPort(5580);
    });

await builder.RunOaktonCommands(args);
```
E l'*Handler* o *Consumer*:
```cs
public class ItemCreatedConsumer
{
    public async Task Consume(ItemCreated itemCreated)
    {
        await Console.Out.WriteLineAsync($@"

    Faccio qualcosa dopo che:
        {itemCreated.Id}
    è stato creato.

");
    }
}
```
Quando riceverà i messaggi sulla porta 5580 gestirà il messaggio come definito dal *Consumer*.

Ovviamente si può impostare come *Durable Inbox* per salvarsi i dati in sicurezza sul database.

### Problemi
Non ho capito esattamente come si gestisce la situazione in cui il *Consumer* non ascolti o sia in *down*.
Sebra che Wolverine tenti più volte di inviare il messaggio e poi lo segni come *handled* e quindi non lo considera più. 
Ho provato a cambiargli comportamento ma non ci sono riuscito.

## Schedulare messaggi o ritornare più messaggi
per schedulare i messaggi in un momento definito basta modificare aggiungendo: `.ScheduledAt(DateTimeOffset);`
come esempio:
```cs
[Transactional]
public static class CreateItemHandler
{
    public static async Task<ScheduledMessage<ItemCreated>> Handle(CreateItemCommand command, ItemDbContext dbContext)
    {
        Item item = command.Item;
        item.Id = Guid.NewGuid();

        dbContext.Items.Add(item);

        await Console.Out.WriteLineAsync($@"

    ITEM CREATO: {item.Id}

");

        return new ItemCreated((Guid)item.Id).ScheduledAt(DateTimeOffset.Now.AddSeconds(10));
    }
}
```

Non bisogna cambiare nulla su `InvokeAsync<T>()`, nel caso in cui s voglia che si ritorni il valore ritornera il valore segnato come T.

Nel caso si vogliano far partire più messaggi si potrebbe utilizzare gli [*OutgoingMessages*](https://wolverine.netlify.app/guide/handlers/cascading.html#using-outgoingmessages).

## Altro
Ci sarebbero altre cose che Wolverine potrebbe implementare come:

Wolverine può anche fungere da [*HTTP Endpoint*](https://wolverine.netlify.app/guide/http/messaging.html#publishing-messages-from-http-endpoints)
Wolverine può implementare il suo sistema di [*Russian Doll Middleware*](https://wolverine.netlify.app/guide/handlers/middleware.html#middleware) 
