SELECT
                                 o.Id, o.CustomerId,
                                 o.UserPaymentTypeId, p.Title, p.DateAdded, p.[Description], p.Id, p.Price, p.ProductTypeId
                            FROM
                            [Order] o
                            LEFT JOIN
                            Product p
                            ON p.CustomerId = o.CustomerId
                            WHERE o.CustomerId = p.CustomerId