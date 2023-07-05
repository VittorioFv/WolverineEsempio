# Analisi della libreria Wolverine
## Confronto con MediatR

Nonostante Wolverine non sia nata con lo scopo di essere utilizzato come mediator può essere usata anche con questo scopo.

Di seguito alcune delle differenze con MediatR, ci tengo a precisare però che Wolverine ha molte più funzionalita e [cerca di coprire molti più casi d'uso](https://jeremydmiller.com/2023/06/19/wolverines-middleware-strategy-is-a-different-animal/), soprattutto come *Message Bus*.

Questa è forse la differenza principale da tenere in considerazione perchè essendo "più grande" aggiunge inevitabilmente anche più complessità; complessità che per progetti piccoli o che non necessitano delle funzionalità aggiuntive di Wolverine potrebbe essere eccessiva.

| Wolverine | MediatR |
|-----------|---------|
| Non era inizialmente progettato per essere usato come [*mediator*](https://wolverine.netlify.app/guide/http/mediator.html#using-as-mediator). | Progettato praticamente per essere utilizzato come *mediator*. |
Wolverine usa la *naming convention* per trovare e assegnare gli [*handler*](https://wolverine.netlify.app/guide/handlers/discovery.html#handler-type-discovery). | MediatR necessita che vengano implementate delle interfaccie.
Supporta e incoraggia la [*method injection*](https://wolverine.netlify.app/tutorials/best-practices.html#best-practices) con classi e metodi di tipo *static*, ma supporta anche la *constructor injection*. | Solo *constuctor injection*.
| Gli handler di wolverine possono essere sia [asincroni che sincroni](https://wolverine.netlify.app/guide/handlers/return-values.html#return-values). | supporta solo metodi [asincroni](https://github.com/jbogard/MediatR/issues/267).
| Potresti avere [più handler](https://wolverine.netlify.app/guide/handlers/#message-handlers) per la stessa richiesta, verranno chiamati in sequenza in base a come vengono trovati. | Non puoi avere [più *IRequestHandler*](https://stackoverflow.com/questions/65151415/mediatr-multiple-requesthandlers) per una richiesta, ma si potrebbero usare [*INotificationHandler*](https://github.com/jbogard/MediatR/issues/163). 
| Non molto documentato e molta della documentazione ufficiale deve ancora essere prodotta ([1](https://wolverine.netlify.app/guide/durability/marten/operations.html#marten-operation-side-effects), [2](https://wolverine.netlify.app/guide/messaging/listeners.html#message-listeners), [3](https://wolverine.netlify.app/guide/messaging/transports/azureservicebus/deadletterqueues.html), [4](https://wolverine.netlify.app/guide/messaging/transports/sqs/queues.html), [5](https://wolverine.netlify.app/guide/messaging/broadcast-to-topic.html#scheduling-message-delivery)). In conpenso all'interno del progetto github ci sono diversi [esempi](https://wolverine.netlify.app/guide/samples.html#sample-projects) (alcuni non ancora spiegati o utilizzati nella documentazione). | Community molto ampia e molte risorse online non solo quelle ufficiali. 
| OGNI valore (ad eccezione dei tipi primitivi) che un *handler* ritorna viene gestito come *Cascading Messages* e quindi vengono inviati ad altri handler o in qualche message queue o bus in base alla configurazione. | il valore viene ritornato normalmente nel caso di *IRequestHandler* mentre per *INotificationHandler* [non dovrebbe restistuire un valore](https://github.com/jbogard/MediatR/issues/163).
| [Local message bus](https://wolverine.netlify.app/guide/messaging/transports/local.html) | 
| [asynchronous messaging framework](https://wolverine.netlify.app/guide/messaging/introduction.html) | 
| Con la libreria [WolverineFx.Http](https://wolverine.netlify.app/guide/http/#http-services-with-wolverine), Wolverine potrebbe essere utilizzata come alternativa di *ASP.Net Core Endpoint provider* (Non analizzerò questo caso).

## Esempio di 2 handler che creano un Item utilizzando Entity framework

### MediatR
https://github.com/VittorioFv/WolverineEntityFrameworkExample/blob/1022d90983667dc26e2ae4bf0a710181a2cfe522/WolverineExample/WolverineAPI/Handler/CreateItemCommand.cs#L5?plain=1

### Wolverine
Comando