const express = require('express');
const cors = require('cors');
const { DefaultAzureCredential } = require('@azure/identity');
const { ServiceBusClient } = require('@azure/service-bus');
require('dotenv').config();
const app = express();
app.use(cors());
app.use(express.json());

app.post('/api/locacao', async (req, res) => {
    const { nome, email, modelo, ano, tempoAluguel } = req.body;
    const connectionString = 'AZURE_SERVICE_BUS_CONNECTION_STRING';
    
    const mensagem = {
        nome,
        email,
        modelo, 
        ano,
        tempoAluguel,
        data: new Date().toISOString(),
    };

    try {
        const crendenciais = new DefaultAzureCredential();
        const serviceBusConnection = connectionString;
        const queueName = 'fila-locacao-auto-queue';
        const sbClient = new ServiceBusClient(serviceBusConnection);
        const sender = sbClient.createSender(queueName);
        const message = {
            body: mensagem,
            label: 'locacao',
            contentType: 'application/json',
        };

        await sender.sendMessages(message);
        await sender.close();
        await sbClient.close();

        res.status(201).json({ message: 'Locação de Veículo enviada para fila com sucesso!' });
    } catch (error) {
        console.log('Erro ao enviar mensagem:', error);
        res.status(500).json({ error: 'Erro ao enviar mensagem' });
    }
});

app.listen(3001, () => {
    console.log('Servidor rodando na porta 3001');
});